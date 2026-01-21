#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GaussianSplatting.Runtime;

public static class BuildChunksFromCsv
{
    // === ANPASSEN ===
    const string MetaCsvPath   = "Assets/SplatChunks/chunks_meta.csv"; // name,minx,miny,minz,maxx,maxy,maxz,count
    const string AssetsFolder  = "Assets/SplatChunks/Assets";          // hier liegen die chunk_*.asset
    const string RootName      = "GS_Chunks";
    const bool   AddBoxCollider = true;
    const bool   MoveWrapperToBoundsCenter = true; // wichtig!
    // =================

    [MenuItem("Tools/Gaussian Splats/Build Chunks From CSV (Fixed)")]
    public static void Build()
    {
        if (!File.Exists(MetaCsvPath))
        {
            Debug.LogError($"CSV not found: {MetaCsvPath}");
            return;
        }

        var root = GameObject.Find(RootName);
        if (!root) root = new GameObject(RootName);

        var rendererType = FindType("GaussianSplatting.Runtime.GaussianSplatRenderer");
        if (rendererType == null)
        {
            Debug.LogError("GaussianSplatRenderer type not found. Check Aras-p package.");
            return;
        }

        var (assetField, assetProp) = FindAssetSlot(rendererType);
        if (assetField == null && assetProp == null)
        {
            Debug.LogError("Could not locate a GaussianSplatAsset field/property on GaussianSplatRenderer.");
            DumpRendererSlots(rendererType);
            return;
        }

        // Assets indexieren: key = Dateiname ohne Extension (stabiler als a.name)
        var guids = AssetDatabase.FindAssets("t:GaussianSplatAsset", new[] { AssetsFolder });
        var assetByKey = new Dictionary<string, GaussianSplatAsset>(StringComparer.Ordinal);
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(path);
            if (!asset) continue;

            var key = Path.GetFileNameWithoutExtension(path); // z.B. tile_0_0_a_b
            if (!assetByKey.ContainsKey(key))
                assetByKey[key] = asset;
        }

        Debug.Log($"Found {assetByKey.Count} GaussianSplatAssets in {AssetsFolder}");

        var lines = File.ReadAllLines(MetaCsvPath);
        if (lines.Length < 2)
        {
            Debug.LogError("CSV has no rows.");
            return;
        }

        int createdWrappers = 0, wired = 0, missingAssets = 0, badRows = 0;
        int loggedMissing = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var c = line.Split(',');
            if (c.Length < 8)
            {
                badRows++;
                continue;
            }

            string name = c[0];

            float minx = ParseF(c[1]);
            float miny = ParseF(c[2]);
            float minz = ParseF(c[3]);
            float maxx = ParseF(c[4]);
            float maxy = ParseF(c[5]);
            float maxz = ParseF(c[6]);

            Vector3 bmin = new Vector3(minx, miny, minz);
            Vector3 bmax = new Vector3(maxx, maxy, maxz);
            Vector3 center = (bmin + bmax) * 0.5f;
            Vector3 size   = (bmax - bmin);

            // Wrapper
            var wrapperT = root.transform.Find(name);
            GameObject wrapper;
            if (!wrapperT)
            {
                wrapper = new GameObject(name);
                wrapper.transform.SetParent(root.transform, false);
                createdWrappers++;
            }
            else wrapper = wrapperT.gameObject;

            // Wrapper zentrieren (Collider lokal korrekt)
            if (MoveWrapperToBoundsCenter)
                wrapper.transform.localPosition = center;

            if (AddBoxCollider)
            {
                var bc = wrapper.GetComponent<BoxCollider>();
                if (!bc) bc = wrapper.AddComponent<BoxCollider>();
                bc.isTrigger = true;
                bc.center = MoveWrapperToBoundsCenter ? Vector3.zero : center;
                bc.size   = size;
            }

            // Asset finden: exakter key = name
            if (!assetByKey.TryGetValue(name, out var asset))
            {
                // fallback: manche Assets heißen leicht anders (z.B. name + ".asset" ist egal, aber key ist ohne ext)
                // optional: Contains match
                var hit = assetByKey.Keys.FirstOrDefault(k => k.Equals(name, StringComparison.Ordinal));
                if (hit == null)
                {
                    missingAssets++;
                    if (loggedMissing < 10)
                    {
                        Debug.LogWarning($"Missing asset for '{name}'. Example existing key: {(assetByKey.Count>0 ? assetByKey.Keys.First() : "<none>")}");
                        loggedMissing++;
                    }
                    continue;
                }
                asset = assetByKey[hit];
            }

            // Child mit Renderer
            var child = wrapper.transform.Find("Splat");
            if (!child)
            {
                var go = new GameObject("Splat");
                go.transform.SetParent(wrapper.transform, false);
                child = go.transform;
            }

            var rend = child.GetComponent(rendererType);
            if (!rend) rend = child.gameObject.AddComponent(rendererType);

            if (assetField != null) assetField.SetValue(rend, asset);
            else assetProp.SetValue(rend, asset);

            wired++;
        }

        Debug.Log($"Done. createdWrappers={createdWrappers}, wired={wired}, missingAssets={missingAssets}, badRows={badRows}");
        Selection.activeGameObject = root;
    }

    static float ParseF(string s)
    {
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            return v;
        return 0f;
    }

    static Type FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName);
            if (t != null) return t;
        }
        return null;
    }

    static (FieldInfo, PropertyInfo) FindAssetSlot(Type rendererType)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (var f in rendererType.GetFields(flags))
            if (typeof(GaussianSplatAsset).IsAssignableFrom(f.FieldType))
                return (f, null);

        foreach (var p in rendererType.GetProperties(flags))
            if (typeof(GaussianSplatAsset).IsAssignableFrom(p.PropertyType) && p.CanWrite)
                return (null, p);

        return (null, null);
    }

    static void DumpRendererSlots(Type rendererType)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Debug.Log("Renderer fields:");
        foreach (var f in rendererType.GetFields(flags))
            Debug.Log($"  {f.FieldType.FullName} {f.Name}");

        Debug.Log("Renderer properties:");
        foreach (var p in rendererType.GetProperties(flags))
            Debug.Log($"  {p.PropertyType.FullName} {p.Name} (CanWrite={p.CanWrite})");
    }
}
#endif