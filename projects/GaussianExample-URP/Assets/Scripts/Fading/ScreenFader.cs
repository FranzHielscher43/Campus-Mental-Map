using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float duration = 0.25f;

    private Coroutine _co;

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
        if (!group) group = GetComponentInChildren<CanvasGroup>(true);
    }

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        if (!group) group = GetComponentInChildren<CanvasGroup>(true);

        if (!group)
        {
            Debug.LogError("[ScreenFader] Kein CanvasGroup gefunden. Bitte CanvasGroup am FadePanel hinzufügen und hier zuweisen.");
            enabled = false;
            return;
        }

        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    public void FadeOut() => StartFade(1f);
    public void FadeIn()  => StartFade(0f);

    public void StartFade(float target)
    {
        if (!isActiveAndEnabled || !group) return;
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(FadeTo(target));
    }

    public IEnumerator FadeTo(float target)
    {
        if (!group) yield break;

        float start = group.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            k = k * k * (3f - 2f * k); // smooth
            group.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }

        group.alpha = target;
        _co = null;
    }
}