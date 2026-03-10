using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class SplatTileStreamer : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead;  
    public Transform tilesRoot;    

    [Header("Tiling")]
    public float tileSize = 5f;
    public int neighborRadius = 0; 

    [Header("Performance")]
    public float keepAliveSeconds = 0.25f;
    public int updateEveryNFrames = 2;

    [Header("Renderer Component")]
    [Tooltip("GENAUER Klassenname der Renderer-Komponente am Leaf-Chunk (wie im Inspector), z.B. GaussianSplatRenderer")]
    public string rendererComponentTypeName = "REPLACE_ME";

    private readonly Dictionary<(int,int), List<Behaviour>> tileRenderers = new();
    private readonly Dictionary<(int,int), float> lastWantedTime = new();
    private readonly HashSet<(int,int)> activeKeys = new();

    private (int,int) lastCenter = (int.MinValue, int.MinValue);
    private Type rendererType;
    private int frame;

    void Awake()
    {
        if (!tilesRoot || !playerHead)
        {
            Debug.LogError("[SplatTileStreamer_RendererToggle] playerHead/tilesRoot fehlt.");
            return;
        }

        rendererType = FindType(rendererComponentTypeName);
        if (rendererType == null)
        {
            Debug.LogError($"[SplatTileStreamer_RendererToggle] Renderer-Typ nicht gefunden: '{rendererComponentTypeName}'. " +
                           $"Trage den exakten Klassennamen ein (wie im Inspector).");
            return;
        }

        foreach (Transform tileGroup in tilesRoot)
        {
            if (!TryParseTileGroupName(tileGroup.name, out int ix, out int iz))
                continue;

            var key = (ix, iz);

            var comps = tileGroup.GetComponentsInChildren(rendererType, true);

            var list = new List<Behaviour>(comps.Length);
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] is Behaviour b)
                    list.Add(b);
            }

            tileRenderers[key] = list;

            SetEnabled(list, false);
        }
    }

    void Update()
    {
        if (rendererType == null) return;
        if ((frame++ % Mathf.Max(1, updateEveryNFrames)) != 0) return;

        Vector3 p = tilesRoot.InverseTransformPoint(playerHead.position);
        int ix = Mathf.FloorToInt(p.x / tileSize);
        int iz = Mathf.FloorToInt(p.z / tileSize);

        var center = (ix, iz);
        float now = Time.time;

        for (int dx = -neighborRadius; dx <= neighborRadius; dx++)
        for (int dz = -neighborRadius; dz <= neighborRadius; dz++)
        {
            var key = (ix + dx, iz + dz);
            lastWantedTime[key] = now;
        }

        if (center != lastCenter)
        {
            lastCenter = center;

            for (int dx = -neighborRadius; dx <= neighborRadius; dx++)
            for (int dz = -neighborRadius; dz <= neighborRadius; dz++)
            {
                var key = (ix + dx, iz + dz);
                if (activeKeys.Contains(key)) continue;

                if (tileRenderers.TryGetValue(key, out var list))
                {
                    SetEnabled(list, true);
                    activeKeys.Add(key);
                }
            }
        }

        if (keepAliveSeconds > 0f)
        {
            tmpKeys.Clear();
            foreach (var k in activeKeys) tmpKeys.Add(k);

            foreach (var k in tmpKeys)
            {
                if (!lastWantedTime.TryGetValue(k, out var t)) t = -999f;
                if (now - t <= keepAliveSeconds) continue;

                if (tileRenderers.TryGetValue(k, out var list))
                    SetEnabled(list, false);

                activeKeys.Remove(k);
            }
        }
    }

    static void SetEnabled(List<Behaviour> list, bool on)
    {
        for (int i = 0; i < list.Count; i++)
        {
            var b = list[i];
            if (b && b.enabled != on)
                b.enabled = on;
        }
    }

    static bool TryParseTileGroupName(string name, out int ix, out int iz)
    {
        ix = iz = 0;
        if (!name.StartsWith("tile_")) return false;
        var parts = name.Split('_');
        if (parts.Length < 3) return false;

        return int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out ix)
            && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out iz);
    }

    static Type FindType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName) || typeName == "REPLACE_ME")
            return null;

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(typeName);
            if (t != null) return t;

            foreach (var tt in asm.GetTypes())
                if (tt.Name == typeName) return tt;
        }
        return null;
    }

    static readonly List<(int,int)> tmpKeys = new List<(int,int)>(128);
}