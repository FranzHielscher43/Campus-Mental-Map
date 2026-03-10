using System.Collections;
using UnityEngine;
using TMPro;

public class ScreenFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup group;

    [Header("UI Text")]
    [SerializeField] private TMP_Text fadeText;

    [Header("Audio (Transition)")]
    [SerializeField] private AudioSource fadeAudio;
    [SerializeField] private float audioFadeDuration = 0.5f;
    [SerializeField] private float audioMaxVolume = 1f;

    [Header("Audio (Other to duck)")]
    [Tooltip("Das eine andere AudioSource-Objekt (z.B. Ambient/Music), das während des Fades leiser werden soll.")]
    [SerializeField] private AudioSource otherAudio;
    [SerializeField] private float otherAudioFadeDuration = 0.35f;
    [SerializeField] private float otherMinVolume = 0f;
    [SerializeField] private float otherNormalVolume = 1f;

    [Header("Timing")]
    [SerializeField] private float duration = 0.25f;

    [Header("Startup")]
    [Tooltip("Wenn true: startet die Szene schwarz und blendet automatisch ein.")]
    [SerializeField] private bool fadeInOnStart = true;

    [Tooltip("Start-Alpha (1 = schwarz, 0 = transparent). Für Auto-FadeIn meist 1.")]
    [Range(0f, 1f)]
    [SerializeField] private float startAlpha = 1f;

    private Coroutine fadeCo;
    private Coroutine audioCo;       
    private Coroutine otherAudioCo; 

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
        if (!group) group = GetComponentInChildren<CanvasGroup>(true);
    }

    void Awake()
    {
        var existing = FindFirstObjectByType<ScreenFader>();
        if (existing != null && existing != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        if (!group) group = GetComponent<CanvasGroup>();
        if (!group) group = GetComponentInChildren<CanvasGroup>(true);

        if (!group)
        {
            Debug.LogError("[ScreenFader] Kein CanvasGroup gefunden. Bitte CanvasGroup am FadePanel hinzufügen und hier zuweisen.");
            enabled = false;
            return;
        }

        var settings = FindFirstObjectByType<SceneSettings>();
        group.alpha = (settings != null && settings.disableFadeIn) ? 0f : startAlpha;

        group.blocksRaycasts = false;
        group.interactable = false;

        if (fadeAudio) fadeAudio.playOnAwake = false;
    }

    void Start()
    {
        var settings = FindFirstObjectByType<SceneSettings>();
        if (settings != null && settings.disableFadeIn)
        {
            StopFade();
            group.alpha = 0f;
            return;
        }

        if (fadeInOnStart)
            FadeIn();
    }

    public void FadeOut()
    {
        Debug.Log($"[FaderAudio] src={(fadeAudio?fadeAudio.name:"null")} clip={(fadeAudio && fadeAudio.clip ? fadeAudio.clip.name:"null")} vol={(fadeAudio?fadeAudio.volume:-1)} playing={(fadeAudio?fadeAudio.isPlaying:false)} spatial={(fadeAudio?fadeAudio.spatialBlend:-1)}");
        Debug.Log($"[ScreenFader] FadeOut called | audio={(fadeAudio?fadeAudio.name:"null")} clip={(fadeAudio && fadeAudio.clip ? fadeAudio.clip.name:"null")} playing={(fadeAudio?fadeAudio.isPlaying:false)} vol={(fadeAudio?fadeAudio.volume:-1)}");
        var settings = FindFirstObjectByType<SceneSettings>();
        if (fadeText)
            fadeText.text = (settings != null && !string.IsNullOrEmpty(settings.fadeOutText)) ? settings.fadeOutText : "Lade...";

        StartFade(1f);
    }

    public void FadeIn()
    {
        var settings = FindFirstObjectByType<SceneSettings>();
        if (fadeText)
            fadeText.text = (settings != null && !string.IsNullOrEmpty(settings.fadeInText)) ? settings.fadeInText : "";

        StartFade(0f);
    }

    public void StartFade(float target)
    {
        if (!isActiveAndEnabled || !group) return;

        if (fadeAudio)
        {
            if (target >= 0.99f)
            {
                if (!fadeAudio.isPlaying)
                {
                    fadeAudio.volume = 0f;
                    fadeAudio.Play();
                }
                StartAudioFade(audioMaxVolume, stopAtEnd: false);
            }
            else if (target <= 0.01f)
            {
                if (fadeAudio.isPlaying)
                    StartAudioFade(0f, stopAtEnd: true);
            }
        }

        if (otherAudio)
        {
            if (target >= 0.99f)
            {
                StartOtherAudioFade(otherMinVolume);
            }
            else if (target <= 0.01f)
            {
                StartOtherAudioFade(otherNormalVolume);
            }
        }

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeTo(target));
    }

    public IEnumerator FadeOutAndWait()
    {
        FadeOut();
        yield return new WaitForSecondsRealtime(duration);
    }

    private void StopFade()
    {
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = null;
    }

    private void StartAudioFade(float targetVolume, bool stopAtEnd)
    {
        if (audioCo != null) StopCoroutine(audioCo);
        audioCo = StartCoroutine(FadeAudioTo(fadeAudio, targetVolume, audioFadeDuration, stopAtEnd));
    }

    private void StartOtherAudioFade(float targetVolume)
    {
        if (otherAudioCo != null) StopCoroutine(otherAudioCo);
        otherAudioCo = StartCoroutine(FadeAudioTo(otherAudio, targetVolume, otherAudioFadeDuration, stopAtEnd: false));
    }

    private IEnumerator FadeAudioTo(AudioSource src, float targetVolume, float fadeDuration, bool stopAtEnd)
    {
        if (!src) yield break;

        float start = src.volume;
        float t = 0f;
        float d = Mathf.Max(0.001f, fadeDuration);

        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / d);
            src.volume = Mathf.Lerp(start, targetVolume, k);
            yield return null;
        }

        src.volume = targetVolume;

        if (stopAtEnd && targetVolume <= 0.001f)
            src.Stop();
    }

    public IEnumerator FadeTo(float target)
    {
        float start = group.alpha;
        float t = 0f;
        float d = Mathf.Max(0.001f, duration);

        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / d);
            k = k * k * (3f - 2f * k);
            group.alpha = Mathf.Lerp(start, target, k);
            yield return null;
        }

        group.alpha = target;
        fadeCo = null;
    }
}