using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject titlescreenRoot;
    public GameObject titlescreenPanel;
    public GameObject aboutUsPanel;
    public GameObject introCanvas; // Dein Slide-Popup
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
        if (introCanvas) introCanvas.SetActive(false); 
        
        if (!head && Camera.main) head = Camera.main.transform;
        if (titlescreenPanel) titlescreenPanel.SetActive(true);
    }

    public void StartApplication()
    {
        Debug.Log("Start gedrückt");
        
        // 1. Menü ausblenden
        if (titlescreenPanel) titlescreenPanel.SetActive(false);
        
        // 2. Intro Canvas aktivieren
        if (introCanvas) 
        {
            introCanvas.SetActive(true);

            // --- REPARATUR ---
            // Wir suchen das Skript auf dem Canvas und zwingen es, SOFORT zu starten!
            // Sonst sind die Panels erst mal unsichtbar.
            var introScript = introCanvas.GetComponent<StartSceneIntroButtonsOnly>();
            if (introScript != null)
            {
                introScript.OpenIntro(); // "Zeig dich sofort!"
            }
        }
        else 
        {
            FinalStartGame();
        }
    }

    public void FinalStartGame()
    {
        if (busy) return;
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        busy = true;
        
        if (introCanvas) introCanvas.SetActive(false);

        if (fader != null)
            yield return fader.FadeTo(1f);
        
        SceneManager.LoadScene(nextScene);
    }

    public void AboutUs()
    {
        if (!titlescreenPanel || !aboutUsPanel) return;
        aboutUsPanel.SetActive(true);
        titlescreenPanel.SetActive(false);
    }

    public void BackFromAboutUs()
    {
        if (!titlescreenPanel || !aboutUsPanel) return;
        aboutUsPanel.SetActive(false);
        titlescreenPanel.SetActive(true);
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