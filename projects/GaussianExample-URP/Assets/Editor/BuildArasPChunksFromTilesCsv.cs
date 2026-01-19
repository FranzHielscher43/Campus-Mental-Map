#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using GaussianSplatting.Runtime;   // <-- aras-p Namespace

public static class BuildArasPChunksFromTilesCsv
{
    // =======================
    // SETTINGS
    // =======================
    const string MetaCsvPath = "Assets/SplatChunks/chunks_meta.csv";
    const string AssetFolder = "Assets/SplatChunks/Assets"; // Ordner mit importierten GaussianSplatAsset (.asset)
    const string RootName    = "Splat_HQ_Tiles";

    const bool CreateTileGroupParents = true;
    const int  MinCountToBuild = 1;
    // =======================

    [MenuItem("Tools/Gaussian Splats/Build ArasP Chunks From chunks_meta.csv")]
    public static void Build()
    {
        if (!File.Exists(MetaCsvPath))
        {
            Debug.LogError($"CSV not found: {MetaCsvPath}");
            return;
        }

        var root = GameObject.Find(RootName);
        if (!root) root = new GameObject(RootName);

        var tileParents = new Dictionary<string, GameObject>();

        var lines = File.ReadAllLines(MetaCsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("CSV has no data rows.");
            return;
        }

        int created = 0, updated = 0, missingAsset = 0, skipped = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var c = line.Split(',');
            if (c.Length < 4) continue;

            string chunkName = c[0].Trim(); // z.B. tile_-1_-1_a_b
            int tileIx  = ParseI(c[1]);
            int tileIz  = ParseI(c[2]);
            int count   = ParseI(c[3]);

            if (count < MinCountToBuild)
            {
                skipped++;
                continue;
            }

            // -------- Parent bestimmen --------
            Transform parentT = root.transform;

            if (CreateTileGroupParents)
            {
                string tileGroupName = $"tile_{tileIx}_{tileIz}";
                if (!tileParents.TryGetValue(tileGroupName, out var tileGO) || !tileGO)
                {
                    var existing = root.transform.Find(tileGroupName);
                    tileGO = existing ? existing.gameObject : new GameObject(tileGroupName);
                    tileGO.transform.SetParent(root.transform, false);
                    tileParents[tileGroupName] = tileGO;
                }
                parentT = tileParents[tileGroupName].transform;
            }

            // -------- Chunk GameObject --------
            var existingChunk = parentT.Find(chunkName);
            GameObject go;
            if (!existingChunk)
            {
                go = new GameObject(chunkName);
                go.transform.SetParent(parentT, false);
                created++;
            }
            else
            {
                go = existingChunk.gameObject;
                updated++;
            }

            // -------- GaussianSplatAsset finden --------
            var splatAsset = FindSplatAsset(chunkName);
            if (!splatAsset)
            {
                Debug.LogWarning($"Missing GaussianSplatAsset for chunk '{chunkName}'");
                missingAsset++;
                continue;
            }

            // -------- GaussianSplatRenderer anbinden --------
            var renderer = go.GetComponent<GaussianSplatRenderer>();
            if (!renderer) renderer = go.AddComponent<GaussianSplatRenderer>();

            // *** ROBUST: Asset per SerializedObject setzen ***
            AssignSplatAsset(renderer, splatAsset);
        }

        Debug.Log(
            $"[BuildArasPChunksFromTilesCsv] Done. " +
            $"created={created}, updated={updated}, missingAsset={missingAsset}, skipped={skipped}"
        );

        Selection.activeGameObject = root;
    }

    // ============================================================
    // HILFSFUNKTIONEN
    // ============================================================

    static GaussianSplatAsset FindSplatAsset(string chunkName)
    {
        // sucht z.B. tile_-1_-1_a_b als GaussianSplatAsset
        string[] guids = AssetDatabase.FindAssets($"{chunkName} t:GaussianSplatAsset", new[] { AssetFolder });
        if (guids.Length == 0) return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(path);
    }

    static void AssignSplatAsset(GaussianSplatRenderer renderer, GaussianSplatAsset asset)
    {
        var so = new SerializedObject(renderer);
        var prop = so.GetIterator();

        bool assigned = false;

        while (prop.NextVisible(true))
        {
            if (prop.propertyType == SerializedPropertyType.ObjectReference &&
                prop.type.Contains("GaussianSplatAsset"))
            {
                prop.objectReferenceValue = asset;
                assigned = true;
                break;
            }
        }

        if (!assigned)
        {
            Debug.LogError($"Could not assign GaussianSplatAsset on {renderer.name}");
        }
        else
        {
            so.ApplyModifiedProperties();
        }
    }

    static int ParseI(string s)
    {
        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
            return v;
        return 0;
    }
}
#endif