#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GaussianSplatting.Runtime;

public static class BuildChunksFromJson
{
    // === ANPASSEN ===
    const string MetaJsonPath  = "Assets/SplatChunks/chunks_meta.json";
    const string AssetsFolder  = "Assets/SplatChunks/Assets"; // chunk_*.asset
    const string RootName      = "GS_Chunks";

    const bool AddBoxCollider  = true;
    const bool ColliderIsTrigger = true;

    const bool MoveWrapperToBoundsCenter = false;

    // ✅ Collider Padding (damit Trigger früher reagiert)
    const float ColliderPadXZ = 2.0f; // +1 Einheit auf X und Z (gesamt)
    const float ColliderPadY  = 0.0f; // optional
    // =================

    [Serializable] class MetaRoot { public List<MetaChunk> chunks; }
    [Serializable] class MetaChunk
    {
        public string name;
        public float[] aabb_min; // [x,y,z]
        public float[] aabb_max;
        public float[] center;
        public float radius;
        public int count;
        public int ix, iz, sub;
    }

    [MenuItem("Tools/Gaussian Splats/Build Chunks From JSON (aras-p)")]
    public static void Build()
    {
        if (!File.Exists(MetaJsonPath))
        {
            Debug.LogError($"JSON not found: {MetaJsonPath}");
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

        // Assets indexieren: key = Dateiname ohne Extension
        var guids = AssetDatabase.FindAssets("t:GaussianSplatAsset", new[] { AssetsFolder });
        var assetByKey = new Dictionary<string, GaussianSplatAsset>(StringComparer.Ordinal);
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var asset = AssetDatabase.LoadAssetAtPath<GaussianSplatAsset>(path);
            if (!asset) continue;

            var key = Path.GetFileNameWithoutExtension(path);
            if (!assetByKey.ContainsKey(key))
                assetByKey[key] = asset;
        }

        Debug.Log($"Found {assetByKey.Count} GaussianSplatAssets in {AssetsFolder}");

        var json = File.ReadAllText(MetaJsonPath);
        var meta = JsonUtility.FromJson<MetaRoot>(json);
        if (meta?.chunks == null || meta.chunks.Count == 0)
        {
            Debug.LogError("JSON parse failed or 'chunks' is empty.");
            return;
        }

        int createdWrappers = 0, wired = 0, missingAssets = 0;

        foreach (var c in meta.chunks)
        {
            string name = Path.GetFileNameWithoutExtension(c.name); // "chunk_x.._z.._s.."

            var wrapperT = root.transform.Find(name);
            GameObject wrapper;
            if (!wrapperT)
            {
                wrapper = new GameObject(name);
                wrapper.transform.SetParent(root.transform, false);
                createdWrappers++;
            }
            else wrapper = wrapperT.gameObject;

            Vector3 bmin = new Vector3(c.aabb_min[0], c.aabb_min[1], c.aabb_min[2]);
            Vector3 bmax = new Vector3(c.aabb_max[0], c.aabb_max[1], c.aabb_max[2]);
            Vector3 center = (bmin + bmax) * 0.5f;
            Vector3 size = (bmax - bmin);

            // ✅ Padding hinzufügen (früher reagieren)
            size.x += ColliderPadXZ;
            size.z += ColliderPadXZ;
            size.y += ColliderPadY;

            // Wrapper verschieben? In den meisten Chunk-Setups NICHT!
            if (MoveWrapperToBoundsCenter)
                wrapper.transform.localPosition = center;
            else
                wrapper.transform.localPosition = Vector3.zero;

            if (AddBoxCollider)
            {
                var bc = wrapper.GetComponent<BoxCollider>();
                if (!bc) bc = wrapper.AddComponent<BoxCollider>();
                bc.isTrigger = ColliderIsTrigger;
                bc.size = size;

                // Wenn Wrapper nicht verschoben wird, muss Collider auf Center sitzen
                bc.center = MoveWrapperToBoundsCenter ? Vector3.zero : center;
            }

            if (!assetByKey.TryGetValue(name, out var asset))
            {
                missingAssets++;
                continue;
            }

            // Child "Splat"
            var child = wrapper.transform.Find("Splat");
            if (!child)
            {
                var go = new GameObject("Splat");
                go.transform.SetParent(wrapper.transform, false);
                child = go.transform;
            }
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;

            var rend = child.GetComponent(rendererType);
            if (!rend) rend = child.gameObject.AddComponent(rendererType);

            if (assetField != null) assetField.SetValue(rend, asset);
            else assetProp.SetValue(rend, asset);

            // Start disabled -> Trigger-Culler schaltet an
            ((Behaviour)rend).enabled = false;

            wired++;
        }

        Debug.Log($"Done. createdWrappers={createdWrappers}, wired={wired}, missingAssets={missingAssets}");
        Selection.activeGameObject = root;
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