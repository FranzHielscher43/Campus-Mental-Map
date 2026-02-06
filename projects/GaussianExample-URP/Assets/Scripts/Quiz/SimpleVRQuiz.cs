using UnityEngine;
using TMPro; 
using UnityEngine.UI; 
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
    public TextMeshProUGUI questionDisplay; // Der Text für die Fragen
    public TextMeshProUGUI resultDisplay;   // --- NEU: Der Text für das Ergebnis ---
    public TextMeshProUGUI feedbackDisplay; // "Richtig/Falsch" Anzeige
    public Button[] answerButtons; 
    public GameObject nextButtonObj; 

    [Header("Einstellungen")]
    public float autoCloseDelay = 5.0f; // Etwas länger lassen, damit man lesen kann

    // INTERNE VARIABLEN
    private Renderer targetCubeRenderer;
    private Material originalMaterial;
    private Material successMaterial; 
    
    [Header("Fragen Konfiguration")]
    public List<Question> allQuestions; 

    private int currentQuestionIndex = 0;
    private bool quizFinished = false;
    private List<string> wrongQuestionsLog = new List<string>(); 

    void Awake()
    {
        // Würfel finden
        SpawnQuiz foundScript = FindAnyObjectByType<SpawnQuiz>(); 
        if (foundScript != null)
        {
            targetCubeRenderer = foundScript.GetComponent<Renderer>();
            if (targetCubeRenderer != null) originalMaterial = targetCubeRenderer.material;
        }

        // Shader suchen
        Shader shaderToUse = Shader.Find("Universal Render Pipeline/Lit");
        if (shaderToUse == null) shaderToUse = Shader.Find("Standard");

        // Material erstellen
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

        // --- NEU: ZUSTAND ZURÜCKSETZEN ---
        if (questionDisplay != null) questionDisplay.gameObject.SetActive(true); // Frage an
        if (resultDisplay != null) resultDisplay.gameObject.SetActive(false);   // Ergebnis aus

        ShuffleQuestions(); 
        
        nextButtonObj.SetActive(false);
        feedbackDisplay.text = "";
        ShowQuestion();
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

    public void UserSelectedAnswer(int buttonIndex)
    {
        if (quizFinished) return;

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
        
        // Buttons verstecken
        foreach(Button btn in answerButtons) btn.gameObject.SetActive(false);
        nextButtonObj.SetActive(false);
        feedbackDisplay.text = "";

        // --- NEU: UI UMRAÜMEN ---
        // Frage ausblenden
        if (questionDisplay != null) questionDisplay.gameObject.SetActive(false);
        
        // Ergebnis einblenden
        if (resultDisplay != null) resultDisplay.gameObject.SetActive(true);

        string resultText = "<b>Quiz beendet!</b>\n\n";

        if (wrongQuestionsLog.Count == 0)
        {
            resultText += "<color=green>Perfekt! Alles richtig.</color>";
            
            if (targetCubeRenderer != null && successMaterial != null)
                targetCubeRenderer.material = successMaterial;

            if (audioSource != null && winFanfareSound != null)
                audioSource.PlayOneShot(winFanfareSound);
        }
        else
        {
            resultText += $"Du musst diese Themen wiederholen ({wrongQuestionsLog.Count} Fehler):\n\n";
            foreach (string qText in wrongQuestionsLog)
            {
                resultText += $"<color=#FF5555>- {qText}</color>\n";
            }
        }

        // Text in das NEUE Feld schreiben
        if (resultDisplay != null)
        {
            resultDisplay.text = resultText;
        }
        else
        {
            // Fallback, falls du vergisst das Feld zuzuweisen
            if(questionDisplay != null) {
                questionDisplay.gameObject.SetActive(true);
                questionDisplay.text = resultText;
            }
        }

        StartCoroutine(CloseCanvasRoutine());
    }

    IEnumerator CloseCanvasRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        gameObject.SetActive(false);
    }
}