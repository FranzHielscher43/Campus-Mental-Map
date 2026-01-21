using System.Collections;
using UnityEngine;

public class ScreenToggle : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;      // z.B. dein Panel
    [SerializeField] private CanvasGroup canvasGroup;   // CanvasGroup am Panel
    [SerializeField] private float duration = 0.2f;     // 0.15 - 0.3 ist VR-angenehm
    [SerializeField] private float hiddenScale = 0.95f; // leicht kleiner beim Ausblenden
    [SerializeField] private bool startHidden = false;

    [Header("Anti-Flicker")]
    [SerializeField] private float cooldown = 0.25f;    // NEU: verhindert Mehrfach-Toggles
    [SerializeField] private bool lockWhileAnimating = true; // NEU: ignoriert Toggles während Fade

    Coroutine _anim;
    Vector3 _shownScale;

    // NEU
    bool _isAnimating;
    float _nextAllowedToggleTime;

    void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;
        if (canvasGroup == null) canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = panelRoot.AddComponent<CanvasGroup>();

        _shownScale = panelRoot.transform.localScale;

        panelRoot.SetActive(true); // bleibt aktiv, wir verstecken via CanvasGroup

        if (startHidden)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            panelRoot.transform.localScale = _shownScale * hiddenScale;
        }
        else
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            panelRoot.transform.localScale = _shownScale;
        }
    }

    public void Toggle()
    {
        // NEU: cooldown
        if (Time.unscaledTime < _nextAllowedToggleTime) return;

        // NEU: lock während Animation (optional)
        if (lockWhileAnimating && _isAnimating) return;

        _nextAllowedToggleTime = Time.unscaledTime + cooldown;

        // NEU: Entscheide anhand des ZIELS (nicht alpha), damit Zwischenzustände nicht flippen
        bool currentlyVisible = canvasGroup.alpha >= 0.5f;
        bool show = !currentlyVisible;

        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Animate(show));
    }

    IEnumerator Animate(bool show)
    {
        _isAnimating = true;
        panelRoot.SetActive(true);

        float startA = canvasGroup.alpha;
        float endA = show ? 1f : 0f;

        Vector3 startS = panelRoot.transform.localScale;
        Vector3 endS = show ? _shownScale : _shownScale * hiddenScale;

        // NEU: Während der Animation NICHT klickbar (verhindert Nachklicks/Spam)
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            k = k * k * (3f - 2f * k); // SmoothStep

            canvasGroup.alpha = Mathf.Lerp(startA, endA, k);
            panelRoot.transform.localScale = Vector3.Lerp(startS, endS, k);
            yield return null;
        }

        canvasGroup.alpha = endA;
        panelRoot.transform.localScale = endS;

        // NEU: erst NACH der Animation wieder klickbar machen (nur wenn sichtbar)
        if (show)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        _anim = null;
        _isAnimating = false;
    }
}