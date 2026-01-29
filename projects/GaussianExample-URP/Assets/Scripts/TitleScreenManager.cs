using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject titlescreenRoot;
    public GameObject titlescreenPanel;
    public GameObject aboutUsPanel;
    public Transform head;

    [Header("Fade")]
    public ScreenFader fader;
    bool busy;

    [Header("Next Scene")]
    public string nextScene = "VR_Labor";

    [Header("Placement")]
    public float distance = 1.4f;
    public float heightOffset = -0.1f;

    [Header("Anti-Spam")]
    public float toggleCooldown = 0.25f;

    void Start ()
    {
        if (aboutUsPanel) aboutUsPanel.SetActive(false);
        if (!head && Camera.main) head = Camera.main.transform;
        if (titlescreenPanel) titlescreenPanel.SetActive(true);
    }

    public void StartApplication()
    {
        Debug.Log("Application started");
        if (busy) return;
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        busy = true;
        if (fader != null)
            yield return fader.FadeTo(1f);
        
        SceneManager.LoadScene(nextScene);
    }

    public void AboutUs()
    {
        if (!titlescreenPanel || !aboutUsPanel) return;
        aboutUsPanel.SetActive(true);
        titlescreenPanel.SetActive(false);
        Debug.Log("About us opened");
    }

    public void BackFromAboutUs()
    {
        if (!titlescreenPanel || !aboutUsPanel) return;
        aboutUsPanel.SetActive(false);
        titlescreenPanel.SetActive(true);
        Debug.Log("Back to Titlescreen");
    }

    public void QuitApplication()
    {
        #if UNITY_EDITOR
            Debug.Log("Quit Application");
        #else
            Application.Quit();
        #endif
    }
}
