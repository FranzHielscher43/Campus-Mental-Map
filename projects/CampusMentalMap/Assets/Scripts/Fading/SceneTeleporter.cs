using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{
    [Header("Target")]
    public string targetSceneName;

    [Header("Fade")]
    public ScreenFader fader;

    [Header("Loading")]
    [Tooltip("Minimale Zeit, die der Screen schwarz bleibt (gegen Flackern).")]
    public float minBlackTime = 1f;

    [Tooltip("Wenn true: Szene im Hintergrund laden und erst nach FadeOut aktivieren.")]
    public bool preloadInBackground = true;

    private bool busy;

    public void Interact()
    {
        Debug.Log($"[SceneTeleporter] Interact called on {name} | target={targetSceneName} | busy={busy} | fader={(fader?fader.name:"null")}");
        if (busy) return;
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[SceneTeleporter] targetSceneName leer!");
            return;
        }
        StartCoroutine(Transition(targetSceneName));
    }

    public void Interact(string sceneName)
    {
        if (busy) return;
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneTeleporter] sceneName leer!");
            return;
        }
        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
{
    busy = true;

    try
    {
        if (fader != null)
        {
            Debug.Log("[SceneTeleporter] FadeOut");
            fader.FadeOut();
            yield return new WaitForSecondsRealtime(0.5f);
        }

        float blackStart = Time.unscaledTime;

        Debug.Log($"[SceneTeleporter] LoadSceneAsync('{sceneName}')");
        var op = SceneManager.LoadSceneAsync(sceneName);

        if (op == null)
        {
            Debug.LogError($"[SceneTeleporter] LoadSceneAsync returned null. Scene '{sceneName}' exists & is in Build Settings?");
            yield break;
        }

        if (preloadInBackground)
        {
            op.allowSceneActivation = false;

            float timeoutAt = Time.unscaledTime + 20f;
            while (op.progress < 0.9f)
            {
                if (Time.unscaledTime > timeoutAt)
                {
                    Debug.LogError("[SceneTeleporter] Loading timed out (progress < 0.9). Check scene name/build settings.");
                    yield break;
                }
                yield return null;
            }

            while (Time.unscaledTime - blackStart < minBlackTime)
                yield return null;

            op.allowSceneActivation = true;
            while (!op.isDone)
                yield return null;
        }
        else
        {
            while (!op.isDone)
                yield return null;
        }

        Debug.Log("[SceneTeleporter] Scene load done");

        if (fader != null)
        {
            Debug.Log("[SceneTeleporter] FadeIn");
            fader.FadeIn();
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
    finally
    {
        busy = false;
    }
}
}