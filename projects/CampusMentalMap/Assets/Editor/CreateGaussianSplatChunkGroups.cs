using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CreateGaussianSplatChunkGroups
{
    [MenuItem("Tools/GaussianSplats/Create Chunk Groups from Selected .asset Files")]
    public static void CreateGroups()
    {
        var root = GameObject.Find("RoomChunks");
        if (root == null) root = new GameObject("RoomChunks");

        var rendererType = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a =>
    {
        try { return a.GetTypes(); }
        catch { return Array.Empty<Type>(); }
    })
    .FirstOrDefault(t =>
        typeof(MonoBehaviour).IsAssignableFrom(t) &&
        t.Name == "GaussianSplatRenderer"
    );

        if (rendererType == null)
        {
            Debug.LogError("GaussianSplatRenderer type not found.");
            return;
        }

        var assetPaths = Selection.objects
            .Select(o => AssetDatabase.GetAssetPath(o))
            .Where(p => !string.IsNullOrEmpty(p) && p.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p)
            .ToList();

        if (assetPaths.Count == 0)
        {
            Debug.LogWarning("No .asset files selected.");
            return;
        }

        // Gruppe = alles vor "_I_" (bei dir: ..._I_0_1 usw.)
        string GroupKey(string fileNameNoExt)
        {
            int idx = fileNameNoExt.IndexOf("_I_");
            return idx >= 0 ? fileNameNoExt.Substring(0, idx) : fileNameNoExt;
        }

        var groups = new Dictionary<string, List<string>>();
        foreach (var path in assetPaths)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            var key = GroupKey(name);
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<string>();
                groups[key] = list;
            }
            list.Add(path);
        }

        int createdGroups = 0;
        int createdRenderers = 0;

        foreach (var kv in groups.OrderBy(k => k.Key))
        {
            var groupName = kv.Key;
            var paths = kv.Value;

            var groupGO = new GameObject(groupName);
            groupGO.transform.SetParent(root.transform, false);

            foreach (var path in paths)
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null) continue;

                var child = new GameObject(System.IO.Path.GetFileNameWithoutExtension(path));
                child.transform.SetParent(groupGO.transform, false);

                var comp = child.AddComponent(rendererType);

                // Asset in Renderer setzen (Property oder Field "asset")
                var prop = rendererType.GetProperty("asset") ?? rendererType.GetProperty("Asset");
                if (prop != null && prop.CanWrite)
                {
                    try { prop.SetValue(comp, asset); } catch { }
                }
                else
                {
                    var field = rendererType.GetField("asset") ?? rendererType.GetField("m_Asset");
                    if (field != null)
                    {
                        try { field.SetValue(comp, asset); } catch { }
                    }
                }

                createdRenderers++;
            }

            createdGroups++;
        }

        Debug.Log($"Created {createdGroups} chunk groups and {createdRenderers} renderers under 'RoomChunks'.");
    }
}