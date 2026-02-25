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

        if (fader != null)
        {
            fader.FadeOut();
            yield return new WaitForSecondsRealtime(0.5f);
        }

        float blackStart = Time.unscaledTime;
        AsyncOperation op = null;

        if (preloadInBackground)
        {
            op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
                yield return null;

            while (Time.unscaledTime - blackStart < minBlackTime)
                yield return null;

            op.allowSceneActivation = true;
            while (!op.isDone)
                yield return null;
        }
        else
        {
            op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
                yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        if(fader != null)
        {
            fader.FadeIn();
            yield return new WaitForSecondsRealtime(0.5f);
        }

        busy = false;
    }
}