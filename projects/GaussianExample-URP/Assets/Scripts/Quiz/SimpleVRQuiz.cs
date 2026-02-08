using UnityEngine;
using TMPro; 
using UnityEngine.UI; // Wichtig für Slider
using System.Collections.Generic;
using System.Collections; 

public class SimpleVRQuiz : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        [TextArea] public string questionText; 
        public string[] answers;               
        public int correctAnswerIndex;         
    }

    [Header("Audio Einstellungen")]
    public AudioSource audioSource;   
    public AudioClip correctSound;    
    public AudioClip wrongSound;      
    public AudioClip winFanfareSound; 

    [Header("UI Zuweisungen")]
    public GameObject backgroundPanel; 
    public TextMeshProUGUI questionDisplay; 
    public TextMeshProUGUI resultDisplay;   
    public TextMeshProUGUI feedbackDisplay; 
    public Button[] answerButtons; 
    public GameObject nextButtonObj; 

    [Header("Zeit-Einstellungen")]
    public float timePerQuestion = 10.0f; 
    public Slider timerSlider; 

    [Header("Einstellungen")]
    public float autoCloseDelay = 5.0f; 

    // INTERNE VARIABLEN
    private Renderer targetCubeRenderer;
    private Material originalMaterial;
    private Material successMaterial; 
    
    private float currentTimer;
    private bool isTimerRunning = false;

    [Header("Fragen Konfiguration")]
    public List<Question> allQuestions; 

    private int currentQuestionIndex = 0;
    private bool quizFinished = false;
    private List<string> wrongQuestionsLog = new List<string>(); 

    void Awake()
    {
        SpawnQuiz foundScript = FindAnyObjectByType<SpawnQuiz>(); 
        if (foundScript != null)
        {
            targetCubeRenderer = foundScript.GetComponent<Renderer>();
            if (targetCubeRenderer != null) originalMaterial = targetCubeRenderer.material;
        }

        Shader shaderToUse = Shader.Find("Universal Render Pipeline/Lit");
        if (shaderToUse == null) shaderToUse = Shader.Find("Standard");

        if (shaderToUse != null)
        {
            successMaterial = new Material(shaderToUse);
            successMaterial.color = Color.green;
            successMaterial.EnableKeyword("_EMISSION");
            successMaterial.SetColor("_EmissionColor", Color.green * 2.0f); 
            successMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }

    void OnEnable() 
    {
        if (backgroundPanel != null)
        {
            RectTransform rt = backgroundPanel.GetComponent<RectTransform>();
            if (rt != null) { rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero; rt.localScale = Vector3.one; }
        }

        currentQuestionIndex = 0;
        quizFinished = false;
        wrongQuestionsLog.Clear();
        
        if (targetCubeRenderer != null && originalMaterial != null)
            targetCubeRenderer.material = originalMaterial;

        // UI Reset
        if (questionDisplay != null) questionDisplay.gameObject.SetActive(true); 
        if (resultDisplay != null) resultDisplay.gameObject.SetActive(false);   

        // --- NEU: SLIDER WIEDER SICHTBAR MACHEN ---
        if (timerSlider != null) timerSlider.gameObject.SetActive(true);

        ShuffleQuestions(); 
        
        nextButtonObj.SetActive(false);
        feedbackDisplay.text = "";
        ShowQuestion();
    }

    void Update()
    {
        if (isTimerRunning && !quizFinished)
        {
            currentTimer -= Time.deltaTime;

            if (timerSlider != null)
            {
                timerSlider.value = currentTimer;
                
                // Farbe ändern (Grün -> Rot)
                if (timerSlider.fillRect != null)
                {
                    Image fillImage = timerSlider.fillRect.GetComponent<Image>();
                    if (fillImage != null)
                    {
                        if (currentTimer <= 3.0f) fillImage.color = Color.red;
                        else fillImage.color = Color.green;
                    }
                }
            }

            if (currentTimer <= 0)
            {
                TimeIsUp();
            }
        }
    }

    void ShuffleQuestions()
    {
        for (int i = 0; i < allQuestions.Count; i++)
        {
            Question temp = allQuestions[i];
            int randomIndex = Random.Range(i, allQuestions.Count);
            allQuestions[i] = allQuestions[randomIndex];
            allQuestions[randomIndex] = temp;
        }
    }

    void ShowQuestion()
    {
        if (currentQuestionIndex >= allQuestions.Count)
        {
            EndQuiz();
            return;
        }

        currentTimer = timePerQuestion;
        isTimerRunning = true;
        
        if (timerSlider != null)
        {
            timerSlider.maxValue = timePerQuestion;
            timerSlider.value = timePerQuestion;
        }

        Question q = allQuestions[currentQuestionIndex];
        questionDisplay.text = q.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            TextMeshProUGUI btnText = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (i < q.answers.Length)
            {
                answerButtons[i].gameObject.SetActive(true);
                btnText.text = q.answers[i];
                answerButtons[i].interactable = true;
                answerButtons[i].image.color = Color.white; 
            }
            else answerButtons[i].gameObject.SetActive(false); 
        }
    }

    void TimeIsUp()
    {
        isTimerRunning = false; 

        Question currentQ = allQuestions[currentQuestionIndex];

        feedbackDisplay.text = "<color=red>ZU LANGSAM!</color>";
        if (audioSource != null && wrongSound != null) audioSource.PlayOneShot(wrongSound);

        wrongQuestionsLog.Add(currentQ.questionText + " (Zeitlimit)");

        answerButtons[currentQ.correctAnswerIndex].image.color = Color.green;

        foreach(Button btn in answerButtons) btn.interactable = false;
        nextButtonObj.SetActive(true);
    }

    public void UserSelectedAnswer(int buttonIndex)
    {
        if (quizFinished || !isTimerRunning) return; 

        isTimerRunning = false; 

        Question currentQ = allQuestions[currentQuestionIndex];
        
        if (buttonIndex == currentQ.correctAnswerIndex)
        {
            feedbackDisplay.text = "<color=green>RICHTIG!</color>";
            answerButtons[buttonIndex].image.color = Color.green;
            if (audioSource != null && correctSound != null) audioSource.PlayOneShot(correctSound);
        }
        else
        {
            feedbackDisplay.text = "<color=red>FALSCH!</color>";
            answerButtons[buttonIndex].image.color = Color.red;
            answerButtons[currentQ.correctAnswerIndex].image.color = Color.green; 
            wrongQuestionsLog.Add(currentQ.questionText);
            if (audioSource != null && wrongSound != null) audioSource.PlayOneShot(wrongSound);
        }

        foreach(Button btn in answerButtons) btn.interactable = false;
        nextButtonObj.SetActive(true);
    }

    public void NextQuestion()
    {
        currentQuestionIndex++;
        nextButtonObj.SetActive(false);
        feedbackDisplay.text = "";
        ShowQuestion();
    }

    void EndQuiz()
    {
        quizFinished = true;
        isTimerRunning = false; 
        
        foreach(Button btn in answerButtons) btn.gameObject.SetActive(false);
        nextButtonObj.SetActive(false);
        feedbackDisplay.text = "";

        // UI Umschalten
        if (questionDisplay != null) questionDisplay.gameObject.SetActive(false);
        if (resultDisplay != null) resultDisplay.gameObject.SetActive(true);

        // --- NEU: SLIDER VERSTECKEN ---
        if (timerSlider != null) timerSlider.gameObject.SetActive(false);

        string resultText = "<b>Quiz beendet!</b>\n\n";

        if (wrongQuestionsLog.Count == 0)
        {
            resultText += "<color=green>Perfekt! Alles richtig.</color>";
            if (targetCubeRenderer != null && successMaterial != null) targetCubeRenderer.material = successMaterial;
            if (audioSource != null && winFanfareSound != null) audioSource.PlayOneShot(winFanfareSound);
        }
        else
        {
            resultText += $"Du musst diese Themen wiederholen ({wrongQuestionsLog.Count} Fehler):\n\n";
            foreach (string qText in wrongQuestionsLog)
            {
                resultText += $"<color=#FF5555>- {qText}</color>\n";
            }
        }

        if (resultDisplay != null) resultDisplay.text = resultText;
        else if(questionDisplay != null) { questionDisplay.gameObject.SetActive(true); questionDisplay.text = resultText; }

        StartCoroutine(CloseCanvasRoutine());
    }

    IEnumerator CloseCanvasRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        gameObject.SetActive(false);
    }
}