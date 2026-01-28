using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GaussianSplatting.Runtime; // <-- wichtig

[RequireComponent(typeof(Collider))]
public class ChunkCuller : MonoBehaviour
{
    [Header("aras-p Renderer")]
    public GaussianSplatRenderer splatRenderer;

    [Header("XR")]
    public Transform xrOrigin; // optional

    [Header("Stability")]
    public float exitDisableDelay = 0.8f;

    int _insideCount = 0;
    Coroutine _disableCo;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        // Renderer gezielt finden (typisch: Child heißt "Splat")
        if (!splatRenderer)
            splatRenderer = GetComponentInChildren<GaussianSplatRenderer>(true);

        if (!splatRenderer)
            Debug.LogError($"[ChunkCuller] No GaussianSplatRenderer found under {name}");

        if (splatRenderer)
            splatRenderer.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsFromXR(other)) return;

        _insideCount++;
        if (_disableCo != null) StopCoroutine(_disableCo);

        if (splatRenderer && !splatRenderer.enabled)
            splatRenderer.enabled = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsFromXR(other)) return;

        _insideCount = Mathf.Max(0, _insideCount - 1);
        if (_insideCount == 0)
            _disableCo = StartCoroutine(DisableAfterDelay());
    }

    IEnumerator DisableAfterDelay()
    {
        yield return new WaitForSeconds(exitDisableDelay);
        if (_insideCount == 0 && splatRenderer)
            splatRenderer.enabled = false;
        _disableCo = null;
    }

    bool IsFromXR(Collider other)
    {
        if (!xrOrigin) return true; // fallback
        return other.transform.IsChildOf(xrOrigin);
    }
}