using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition I { get; private set; }

    [Header("References")]
    public ScreenFader fader;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        DontDestroyOnLoad(gameObject);

        if (!fader) fader = GetComponentInChildren<ScreenFader>(true);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(Routine(sceneName));
    }

    IEnumerator Routine(string sceneName)
    {
        if (fader) yield return fader.FadeTo(1f);

        SceneManager.LoadScene(sceneName);

        yield return null;
        yield return null;

        var cam = Camera.main;
        if (cam && fader)
        {
            fader.transform.SetParent(cam.transform, false);
            fader.transform.localPosition = Vector3.zero;
            fader.transform.localRotation = Quaternion.identity;
        }

        if (fader) yield return fader.FadeTo(0f);
    }
}