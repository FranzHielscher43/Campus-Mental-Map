using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Wichtig für Button-Referenzen

public class StartSceneIntroButtonsOnly : MonoBehaviour
{
    [Header("UI-Struktur")]
    public RectTransform[] targetPanels; 
    
    [Header("Navigation Group")]
    public GameObject navigationParent;  
    public GameObject backButton;        
    public GameObject nextButton;        
    public GameObject closeButton;       

    [Header("Auto-Start")]
    public float autoStartDelay = 3.0f; 

    [Header("Animation Settings")]
    public Vector2 hiddenOffset = new Vector2(0, -1000); 
    public Vector2 visiblePosition = Vector2.zero;      
    public float slideDuration = 0.5f; 

    [Header("Anti-Doppelklick (WICHTIG)")]
    public float clickCooldown = 0.5f; // Wartezeit nach Klick
    private float lastClickTime = 0f;  // Wann wurde zuletzt geklickt?

    private bool isOpen = false;
    private int currentSlideIndex = 0;

    void Start()
    {
        if (targetPanels != null && targetPanels.Length > 0)
        {
            // Alle Panels initial verstecken
            for (int i = 0; i < targetPanels.Length; i++) {
                targetPanels[i].anchoredPosition = hiddenOffset;
                targetPanels[i].gameObject.SetActive(false);
            }
            
            if (navigationParent != null) navigationParent.SetActive(false);
            
            isOpen = false;
            StartCoroutine(AutoStartTimer());
        }
    }

    IEnumerator AutoStartTimer()
    {
        yield return new WaitForSeconds(autoStartDelay);
        if (!isOpen) OpenIntro();
    }

    // --- NAVIGATION MIT COOLDOWN ---

    public void NextSlide()
    {
        // 1. NEU: Prüfen, ob wir warten müssen
        if (Time.time - lastClickTime < clickCooldown) return;
        lastClickTime = Time.time; // Zeit merken

        Debug.Log("<color=green>Nav:</color> Nächster Slide");
        
        // Normale Logik weiter...
        if (!isOpen || currentSlideIndex >= targetPanels.Length - 1) return;
        StartCoroutine(SwitchRoutine(currentSlideIndex + 1));
    }

    public void PreviousSlide()
    {
        // 1. NEU: Auch hier die Bremse rein
        if (Time.time - lastClickTime < clickCooldown) return;
        lastClickTime = Time.time;

        Debug.Log("<color=yellow>Nav:</color> Vorheriger Slide");
        
        if (!isOpen || currentSlideIndex <= 0) return;
        StartCoroutine(SwitchRoutine(currentSlideIndex - 1));
    }

    public void CloseIntro()
    {
        // Auch beim Schließen kurz warten, damit man nicht aus Versehen was dahinter anklickt
        if (Time.time - lastClickTime < clickCooldown) return;
        lastClickTime = Time.time;

        Debug.Log("<color=red>Nav:</color> Intro beendet");
        if (!isOpen) return;
        isOpen = false;
        StartCoroutine(SlideRoutine(currentSlideIndex, false));
        if (navigationParent != null) navigationParent.SetActive(false);
    }

    IEnumerator SwitchRoutine(int newIndex)
    {
        // Altes Panel raus
        StartCoroutine(SlideRoutine(currentSlideIndex, false));
        
        currentSlideIndex = newIndex;
        
        // Neues Panel rein
        yield return StartCoroutine(SlideRoutine(currentSlideIndex, true));
        
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        if (backButton != null) backButton.SetActive(currentSlideIndex > 0);
        
        bool isLast = (currentSlideIndex == targetPanels.Length - 1);
        if (nextButton != null) nextButton.SetActive(!isLast);
        
        if (closeButton != null) closeButton.SetActive(isLast);
    }

    public void OpenIntro()
    {
        isOpen = true;
        if (navigationParent != null) navigationParent.SetActive(true);
        
        targetPanels[currentSlideIndex].gameObject.SetActive(true);
        StartCoroutine(SlideRoutine(currentSlideIndex, true));
        UpdateNavigationButtons();
    }

    // --- ANIMATION ---

    IEnumerator SlideRoutine(int index, bool show)
    {
        RectTransform target = targetPanels[index];
        if (show) target.gameObject.SetActive(true);

        float elapsed = 0;
        Vector2 startPos = target.anchoredPosition;
        Vector2 endPos = show ? visiblePosition : hiddenOffset;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / slideDuration);
            target.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        target.anchoredPosition = endPos;
        if (!show) target.gameObject.SetActive(false);
    }
}