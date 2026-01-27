using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class StartSceneIntroButtonsOnly : MonoBehaviour
{
    [Header("UI-Struktur")]
    public RectTransform[] targetPanels; // Deine Slides (slide1, slide1 (1), etc.)
    
    [Header("Navigation Group")]
    public GameObject navigationParent;  // Das Objekt "NavigationUI"
    public GameObject backButton;        // Knopf "Zurück"
    public GameObject nextButton;        // Knopf "weiter"
    public GameObject closeButton;       // Knopf "Close" (erscheint am Ende)

    [Header("Auto-Start")]
    public float autoStartDelay = 3.0f; 

    [Header("Animation Settings")]
    public Vector2 hiddenOffset = new Vector2(0, -1000); 
    public Vector2 visiblePosition = Vector2.zero;      
    public float slideDuration = 0.5f; 

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
            
            // NavigationUI am Anfang aus
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

    // --- NAVIGATION (Wird nur über UI-Buttons aufgerufen) ---

    public void NextSlide()
    {
        Debug.Log("<color=green>Nav:</color> Nächster Slide");
        if (!isOpen || currentSlideIndex >= targetPanels.Length - 1) return;
        StartCoroutine(SwitchRoutine(currentSlideIndex + 1));
    }

    public void PreviousSlide()
    {
        Debug.Log("<color=yellow>Nav:</color> Vorheriger Slide");
        if (!isOpen || currentSlideIndex <= 0) return;
        StartCoroutine(SwitchRoutine(currentSlideIndex - 1));
    }

    public void CloseIntro()
    {
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
        
        // Der Beenden-Button kommt NUR beim letzten Panel
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