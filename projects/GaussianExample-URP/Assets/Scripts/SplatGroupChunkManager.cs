using System;
using System.Collections.Generic;
using UnityEngine;

public class SplatGroupChunkManager : MonoBehaviour
{
    [Header("XR Head (Main Camera)")]
    public Transform head;

    [Header("Activation")]
    public int lookAheadGroups = 2;
    public bool keepNearestGroupOn = true;
    public float maxConsiderDistance = 20f;
    public float keepAliveSeconds = 1.0f;

    private readonly List<Transform> _groups = new();
    private readonly Dictionary<Transform, float> _keepAliveUntil = new();

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;

        _groups.Clear();
        foreach (Transform child in transform)
            _groups.Add(child);

        // Start: alles aus
        foreach (var g in _groups)
            g.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!head) return;

        Vector3 headPos = head.position;
        Vector3 fwd = head.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) return;
        fwd.Normalize();

        // Score pro Gruppe: Blickwinkel + Distanz
        var scored = new List<(Transform g, float score, float dist)>(_groups.Count);
        foreach (var g in _groups)
        {
            if (g == null) continue;

            Vector3 c = g.position; // Gruppen liegen bei (0,0,0) -> besser Center aus Kindern holen
            // Quick & dirty: nimm Position des ersten Kindes als Näherung
            if (g.childCount > 0) c = g.GetChild(0).position;

            Vector3 to = c - headPos;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist > maxConsiderDistance) continue;

            Vector3 dir = to / Mathf.Max(dist, 0.0001f);
            float dot = Vector3.Dot(fwd, dir);
            float distScore = 1f - (dist / maxConsiderDistance);
            float score = dot * 0.75f + distScore * 0.25f;

            scored.Add((g, score, dist));
        }

        scored.Sort((a, b) => b.score.CompareTo(a.score));

        var desired = new HashSet<Transform>();

        if (keepNearestGroupOn)
        {
            Transform nearest = null;
            float best = float.MaxValue;
            foreach (var g in _groups)
            {
                if (g == null) continue;
                Vector3 c = (g.childCount > 0) ? g.GetChild(0).position : g.position;
                float d = Vector2.Distance(new Vector2(headPos.x, headPos.z), new Vector2(c.x, c.z));
                if (d < best) { best = d; nearest = g; }
            }
            if (nearest) desired.Add(nearest);
        }

        for (int i = 0; i < Mathf.Min(lookAheadGroups, scored.Count); i++)
            desired.Add(scored[i].g);

        float now = Time.time;
        foreach (var g in desired)
            _keepAliveUntil[g] = now + keepAliveSeconds;

        foreach (var g in _groups)
        {
            if (g == null) continue;
            bool on = _keepAliveUntil.TryGetValue(g, out float until) && until > now;
            if (g.gameObject.activeSelf != on) g.gameObject.SetActive(on);
        }
    }
}