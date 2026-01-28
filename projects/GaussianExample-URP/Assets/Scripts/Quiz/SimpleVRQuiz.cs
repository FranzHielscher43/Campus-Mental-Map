using UnityEngine;
using TMPro; 
using UnityEngine.UI; 
using System.Collections.Generic;

public class SimpleVRQuiz : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        [TextArea] public string questionText; 
        public string[] answers;               
        public int correctAnswerIndex;         
    }

    [Header("UI Zuweisungen")]
    public TextMeshProUGUI questionDisplay;
    public TextMeshProUGUI feedbackDisplay;
    public Button[] answerButtons; // Hier Button_0, Button_1, Button_2 reinziehen
    public GameObject nextButtonObj; 

    [Header("Fragen Konfiguration")]
    public List<Question> allQuestions; 

    private int currentQuestionIndex = 0;
    private bool quizFinished = false;
    private List<string> wrongQuestionsLog = new List<string>(); // Merkzettel für Fehler

    void OnEnable() // Startet jedes Mal neu, wenn das Panel geöffnet wird
    {
        currentQuestionIndex = 0;
        quizFinished = false;
        wrongQuestionsLog.Clear();
        
        ShuffleQuestions(); // Mischen!
        
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
            else
            {
                answerButtons[i].gameObject.SetActive(false); // Verstecke ungenutzte Buttons
            }
        }
    }

    public void UserSelectedAnswer(int buttonIndex)
    {
        if (quizFinished) return;

        Question currentQ = allQuestions[currentQuestionIndex];
        int correctIndex = currentQ.correctAnswerIndex;

        if (buttonIndex == correctIndex)
        {
            feedbackDisplay.text = "<color=green>RICHTIG!</color>";
            answerButtons[buttonIndex].image.color = Color.green;
        }
        else
        {
            feedbackDisplay.text = "<color=red>FALSCH!</color>";
            answerButtons[buttonIndex].image.color = Color.red;
            answerButtons[correctIndex].image.color = Color.green; // Zeige richtige Lösung
            
            // Fehler protokollieren
            wrongQuestionsLog.Add(currentQ.questionText);
        }

        // Alle Buttons sperren
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
        foreach(Button btn in answerButtons) btn.gameObject.SetActive(false);
        nextButtonObj.SetActive(false);
        feedbackDisplay.text = "";

        string resultText = "<b>Quiz beendet!</b>\n\n";

        if (wrongQuestionsLog.Count == 0)
        {
            resultText += "<color=green>Perfekt! Alles richtig.</color>";
        }
        else
        {
            resultText += $"Du musst diese Themen wiederholen ({wrongQuestionsLog.Count} Fehler):\n\n";
            foreach (string qText in wrongQuestionsLog)
            {
                resultText += $"<color=#FF5555>- {qText}</color>\n";
            }
        }
        questionDisplay.text = resultText;
    }
}