using UnityEngine;
using GaussianSplatting.Runtime;

public class SplatDetailZoneController : MonoBehaviour
{
    public Transform playerHead;
    public GaussianSplatRenderer hqRenderer;

    public float radiusMeters = 5.0f;
    public float hysteresisMeters = 0.35f;

    public bool logEverySecond = true;

    bool hqOn;
    float nextLog;

    void Start()
    {
        if (hqRenderer) hqRenderer.enabled = false;
        hqOn = false;
        Debug.Log("[DetailZone] Start() läuft");
    }

    void Update()
    {
        if (!playerHead || !hqRenderer) return;

        float d = Vector3.Distance(playerHead.position, transform.position);

        bool inside = hqOn
            ? d <= radiusMeters + hysteresisMeters
            : d <= radiusMeters;

        if (inside != hqOn)
        {
            hqOn = inside;
            hqRenderer.enabled = hqOn;
        }

        if (logEverySecond && Time.time >= nextLog)
        {
            nextLog = Time.time + 1f;
            Debug.Log($"[DetailZone] d={d:F2}m radius={radiusMeters:F2} HQ={(hqOn ? "ON" : "OFF")}  zonePos={transform.position} headPos={playerHead.position}");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radiusMeters);
    }
}