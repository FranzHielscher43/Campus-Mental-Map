using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SplatChunkManager : MonoBehaviour
{
    [Header("XR Camera")]
    public Camera vrCamera;

    [Header("Grid (auto-kalibriert)")]
    [Tooltip("Größe EINER Sub-Zelle (chunk_*_*_sub_u_v). Bei dir ~14.39m")]
    public float cellSize = 14.39f;

    [Tooltip("Wird automatisch gesetzt")]
    public Vector2 gridOrigin = Vector2.zero;

    public float boundsHeight = 4f;

    [Header("Sub-Grid Layout")]
    [Tooltip("Wie viele _sub_ Zellen pro grober chunk_X_Y Kachel? (bei dir 4 -> sub 0..3)")]
    public int subDiv = 4;

    [Header("Streaming")]
    public int visibleChunks = 5;         // MAIN tiles
    public float maxDistance = 35f;
    public float keepAliveSeconds = 1.0f;
    public bool loadNeighborRing = true;

    [Header("Debug")]
    public bool logEverySecond = true;
    public bool logGridStatsOnce = true;

    class Chunk
    {
        public Transform root;
        public GameObject renderGO;
        public Bounds boundsWorld;
        public Vector3 centerWorld;

        // coarse
        public int gx, gz;
        // sub
        public int sx, sz;
        // global tile coords (für NeighborRing)
        public int tx, tz;
    }

    // Matches:
    // chunk_3_3_sub_2_1
    // chunk_3_3_sub_2_1_m_0_0
    // chunk_3_3_sub_2_1_l_1_0
    static readonly Regex rx = new Regex(
        @"chunk_(\d+)_(\d+)_sub_(\d+)_(\d+)",
        RegexOptions.Compiled
    );

    readonly List<Chunk> chunks = new();
    readonly Dictionary<(int,int), Chunk> byTile = new(); // key = (tx,tz)
    readonly Dictionary<Chunk, float> keepAlive = new();

    bool loggedStats = false;

    void Awake()
    {
        if (!vrCamera && Camera.main) vrCamera = Camera.main;
        Debug.Log($"[SplatChunkManager] Using camera: {(vrCamera ? vrCamera.name : "NULL")}");
        BuildChunks();
    }

    void Start()
    {
        foreach (var c in chunks)
            c.renderGO.SetActive(false);
    }

    void Update()
    {
        if (!vrCamera) return;

        float now = Time.time;
        Vector3 camPos = vrCamera.transform.position;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(vrCamera);

        var candidates = new List<(Chunk c, float dist)>(chunks.Count);

        foreach (var c in chunks)
        {
            float dist = Vector3.Distance(camPos, c.centerWorld);
            if (dist > maxDistance) continue;

            if (!GeometryUtility.TestPlanesAABB(planes, c.boundsWorld))
                continue;

            candidates.Add((c, dist));
        }

        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));

        int mainCount = Mathf.Min(visibleChunks, candidates.Count);
        var main = new List<Chunk>(mainCount);
        for (int i = 0; i < mainCount; i++)
            main.Add(candidates[i].c);

        var target = new HashSet<Chunk>(main);

        if (loadNeighborRing)
        {
            foreach (var c in main)
            {
                for (int dx = -1; dx <= 1; dx++)
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (byTile.TryGetValue((c.tx + dx, c.tz + dz), out var nb))
                        target.Add(nb);
                }
            }
        }

        foreach (var c in target)
            keepAlive[c] = now + keepAliveSeconds;

        int active = 0;
        foreach (var c in chunks)
        {
            bool on = target.Contains(c) &&
                      keepAlive.TryGetValue(c, out float until) &&
                      until > now;

            if (c.renderGO.activeSelf != on)
                c.renderGO.SetActive(on);

            if (c.renderGO.activeSelf) active++;
        }

        var dead = new List<Chunk>();
        foreach (var kv in keepAlive)
            if (kv.Value <= now) dead.Add(kv.Key);
        foreach (var c in dead) keepAlive.Remove(c);

        if (logEverySecond && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[SplatChunkManager] main {main.Count} | target {target.Count} | active {active}/{chunks.Count} | candidates {candidates.Count}");
        }

        if (logGridStatsOnce && !loggedStats)
        {
            loggedStats = true;

            int minTX=int.MaxValue, minTZ=int.MaxValue, maxTX=int.MinValue, maxTZ=int.MinValue;
            foreach (var c in chunks)
            {
                minTX = Mathf.Min(minTX, c.tx);
                minTZ = Mathf.Min(minTZ, c.tz);
                maxTX = Mathf.Max(maxTX, c.tx);
                maxTZ = Mathf.Max(maxTZ, c.tz);
            }

            Debug.Log($"[SplatChunkManager] tiles tx {minTX}..{maxTX} | tz {minTZ}..{maxTZ} | byTile={byTile.Count}");
        }
    }

    void BuildChunks()
    {
        chunks.Clear();
        byTile.Clear();

        var temp = new List<(Transform t, int gx, int gz, int sx, int sz, GameObject go)>();

        int minTX = int.MaxValue, minTZ = int.MaxValue;
        foreach (Transform t in transform)
        {
            var m = rx.Match(t.name);
            if (!m.Success) continue;

            MonoBehaviour renderer = null;
            foreach (var mb in t.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb && mb.GetType().Name == "GaussianSplatRenderer")
                {
                    renderer = mb;
                    break;
                }
            }
            if (!renderer) continue;

            int gx = int.Parse(m.Groups[1].Value);
            int gz = int.Parse(m.Groups[2].Value);
            int sx = int.Parse(m.Groups[3].Value);
            int sz = int.Parse(m.Groups[4].Value);

            int tx = gx * subDiv + sx;
            int tz = gz * subDiv + sz;

            minTX = Mathf.Min(minTX, tx);
            minTZ = Mathf.Min(minTZ, tz);

            temp.Add((t, gx, gz, sx, sz, t.gameObject));
        }

        Debug.Log($"[SplatChunkManager] chunks found: {temp.Count}");
        if (temp.Count == 0) return;

        Transform refChunk = null;
        int refTX = 0, refTZ = 0;

        foreach (var e in temp)
        {
            int tx = e.gx * subDiv + e.sx;
            int tz = e.gz * subDiv + e.sz;

            if (tx == minTX && tz == minTZ)
            {
                refChunk = e.t;
                refTX = tx;
                refTZ = tz;
                break;
            }
        }

        if (!refChunk)
        {
            refChunk = temp[0].t;
            refTX = temp[0].gx * subDiv + temp[0].sx;
            refTZ = temp[0].gz * subDiv + temp[0].sz;
        }

        gridOrigin = new Vector2(
            refChunk.position.x - (refTX + 0.5f) * cellSize,
            refChunk.position.z - (refTZ + 0.5f) * cellSize
        );

        Debug.Log($"[SplatChunkManager] AUTO gridOrigin=({gridOrigin.x:F2}, {gridOrigin.y:F2}) (minTX={minTX}, minTZ={minTZ})");

        foreach (var e in temp)
        {
            int tx = e.gx * subDiv + e.sx;
            int tz = e.gz * subDiv + e.sz;

            Vector3 center = new Vector3(
                gridOrigin.x + (tx + 0.5f) * cellSize,
                0f,
                gridOrigin.y + (tz + 0.5f) * cellSize
            );

            Vector3 size = new Vector3(cellSize * 1.2f, boundsHeight, cellSize * 1.2f);
            Bounds b = new Bounds(center + Vector3.up * (boundsHeight * 0.5f), size);

            var c = new Chunk
            {
                root = e.t,
                renderGO = e.go,
                gx = e.gx, gz = e.gz,
                sx = e.sx, sz = e.sz,
                tx = tx, tz = tz,
                centerWorld = center,
                boundsWorld = b
            };

            chunks.Add(c);

            var key = (c.tx, c.tz);
            if (byTile.ContainsKey(key))
            {
                Debug.LogWarning($"[SplatChunkManager] Duplicate tile key {key} for {c.root.name} (existing: {byTile[key].root.name})");
            }
            byTile[key] = c;
        }
    }
}