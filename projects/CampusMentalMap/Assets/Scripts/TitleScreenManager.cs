using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject titlescreenRoot;
    public GameObject titlescreenPanel;
    public GameObject aboutUsPanel;
    public GameObject introCanvas; 
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

    private static bool hasSeenIntro = false; 

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetIntroStatus()
    {
        hasSeenIntro = false;
        Debug.Log("Intro-Status wurde zurückgesetzt!");
    }

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

        // --- HIER IST DIE LOGIK FÜR "NUR 1x ANZEIGEN" ---
        
        // Wenn wir ein Intro haben UND es noch NICHT gesehen haben:
        if (introCanvas != null && !hasSeenIntro)
        {
            // Merken, dass wir es jetzt sehen
            hasSeenIntro = true;

            // Intro Canvas anschalten -> Das aktiviert automatisch den Timer im anderen Skript
            introCanvas.SetActive(true);
        }
        else 
        {
            // Wenn wir es schon gesehen haben (oder keins da ist):
            // Sofort starten!
            FinalStartGame();
        }
    }

    // Wird vom Intro-Skript (beim Schließen) ODER direkt von oben aufgerufen
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

    // --- Standard UI ---
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