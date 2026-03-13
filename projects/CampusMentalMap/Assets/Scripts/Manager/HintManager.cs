using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool hintsEnabled = true;

    [Header("Click Safety")]
    [SerializeField] private bool delayOneFrame = true;

    private readonly List<GameObject> _hintRoots = new();

    void Awake()
    {
        _hintRoots.Clear();
        foreach (var m in FindObjectsOfType<HintMarker>(true))
            _hintRoots.Add(m.gameObject);

        ApplyImmediate(hintsEnabled);
    }

    // --- UI Buttons call these ---
    public void HintsOn()  => ApplySafe(true);
    public void HintsOff() => ApplySafe(false);

    void ApplySafe(bool enabled)
    {
        if (delayOneFrame)
            StartCoroutine(ApplyNextFrame(enabled));
        else
            ApplyImmediate(enabled);
    }

    IEnumerator ApplyNextFrame(bool enabled)
    {
        // XR-UI Click sauber abschließen lassen
        yield return null;
        yield return new WaitForEndOfFrame();

        ApplyImmediate(enabled);
    }

    void ApplyImmediate(bool enabled)
    {
        hintsEnabled = enabled;

        for (int i = 0; i < _hintRoots.Count; i++)
            if (_hintRoots[i]) _hintRoots[i].SetActive(enabled);

        Debug.Log($"[HintManager] Hints = {enabled} (count={_hintRoots.Count})");
    }
}