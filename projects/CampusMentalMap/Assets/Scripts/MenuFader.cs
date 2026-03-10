using UnityEngine;
using System.Collections;

public class MenuFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup group;

    [Header("Animation")]
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float hiddenScale = 0.95f;

    Coroutine _co;
    Vector3 _shownScale;

    public bool IsVisible => group != null && group.alpha > 0.99f;

    void Reset()
    {
        root = gameObject;
        group = GetComponent<CanvasGroup>();
    }

    void Awake()
    {
        if (!root) root = gameObject;
        if (!group) group = root.GetComponent<CanvasGroup>();
        if (!group) group = root.AddComponent<CanvasGroup>();

        _shownScale = root.transform.localScale;

        root.SetActive(true);
        SetInstant(false);
    }

    public void Toggle()
    {
        if (group == null) return;
        bool show = group.alpha <= 0.01f;
        if (show) Show();
        else Hide();
    }

    public void Show()
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Animate(true));
    }

    public void Hide()
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Animate(false));
    }

    void SetInstant(bool show)
    {
        if (!root) root = gameObject;
        if (!group) group = root.GetComponent<CanvasGroup>();

        float a = show ? 1f : 0f;
        root.transform.localScale = show ? _shownScale : _shownScale * hiddenScale;
        group.alpha = a;
        group.interactable = show;
        group.blocksRaycasts = show;
    }

    IEnumerator Animate(bool show)
    {
        float d = Mathf.Max(0.01f, duration);

        float startA = group.alpha;
        float endA = show ? 1f : 0f;

        Vector3 startS = root.transform.localScale;
        Vector3 endS = show ? _shownScale : _shownScale * hiddenScale;

        group.interactable = false;
        group.blocksRaycasts = false;

        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / d);
            k = k * k * (3f - 2f * k);

            group.alpha = Mathf.Lerp(startA, endA, k);
            root.transform.localScale = Vector3.Lerp(startS, endS, k);
            yield return null;
        }

        group.alpha = endA;
        root.transform.localScale = endS;

        if (show)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        else
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        _co = null;
    }
}