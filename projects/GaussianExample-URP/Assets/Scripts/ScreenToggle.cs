using System.Collections;
using UnityEngine;

public class ScreenToggle : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;      // z.B. dein Panel
    [SerializeField] private CanvasGroup canvasGroup;   // CanvasGroup am Panel
    [SerializeField] private float duration = 0.2f;     // 0.15 - 0.3 ist VR-angenehm
    [SerializeField] private float hiddenScale = 0.95f; // leicht kleiner beim Ausblenden
    [SerializeField] private bool startHidden = false;

    Coroutine _anim;
    Vector3 _shownScale;

    void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;
        if (canvasGroup == null) canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = panelRoot.AddComponent<CanvasGroup>();

        _shownScale = panelRoot.transform.localScale;

        if (startHidden)
        {
            panelRoot.SetActive(true); // bleibt aktiv, wir verstecken via CanvasGroup
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            panelRoot.transform.localScale = _shownScale * hiddenScale;
        }
        else
        {
            panelRoot.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            panelRoot.transform.localScale = _shownScale;
        }
    }

    public void Toggle()
    {
        bool show = canvasGroup.alpha <= 0.01f;
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Animate(show));
    }

    IEnumerator Animate(bool show)
    {
        panelRoot.SetActive(true);

        float startA = canvasGroup.alpha;
        float endA = show ? 1f : 0f;

        Vector3 startS = panelRoot.transform.localScale;
        Vector3 endS = show ? _shownScale : _shownScale * hiddenScale;

        // beim Einblenden sofort klickbar, beim Ausblenden erst am Ende deaktivieren
        if (show)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // unabhängig von TimeScale
            float k = Mathf.Clamp01(t / duration);
            // SmoothStep fürs weiche Gefühl
            k = k * k * (3f - 2f * k);

            canvasGroup.alpha = Mathf.Lerp(startA, endA, k);
            panelRoot.transform.localScale = Vector3.Lerp(startS, endS, k);
            yield return null;
        }

        canvasGroup.alpha = endA;
        panelRoot.transform.localScale = endS;

        if (!show)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            // optional: panelRoot.SetActive(false);  // würde aber wieder "hart" sein
        }

        _anim = null;
    }
}