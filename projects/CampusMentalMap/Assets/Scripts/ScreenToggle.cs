using System.Collections;
using UnityEngine;

public class ScreenToggle : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;      
    [SerializeField] private CanvasGroup canvasGroup;   
    [SerializeField] private float duration = 0.2f;     
    [SerializeField] private float hiddenScale = 0.95f; 
    [SerializeField] private bool startHidden = false;

    [Header("Anti-Flicker")]
    [SerializeField] private float cooldown = 0.25f;    
    [SerializeField] private bool lockWhileAnimating = true; 

    Coroutine _anim;
    Vector3 _shownScale;

    bool _isAnimating;
    float _nextAllowedToggleTime;

    void Awake()
    {
        if (panelRoot == null) panelRoot = gameObject;
        if (canvasGroup == null) canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = panelRoot.AddComponent<CanvasGroup>();

        _shownScale = panelRoot.transform.localScale;

        panelRoot.SetActive(true); 

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
        if (Time.unscaledTime < _nextAllowedToggleTime) return;

        if (lockWhileAnimating && _isAnimating) return;

        _nextAllowedToggleTime = Time.unscaledTime + cooldown;

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

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            k = k * k * (3f - 2f * k); 

            canvasGroup.alpha = Mathf.Lerp(startA, endA, k);
            panelRoot.transform.localScale = Vector3.Lerp(startS, endS, k);
            yield return null;
        }

        canvasGroup.alpha = endA;
        panelRoot.transform.localScale = endS;

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