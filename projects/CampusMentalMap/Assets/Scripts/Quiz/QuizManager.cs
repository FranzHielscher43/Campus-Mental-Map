using UnityEngine;
using TMPro; // Wichtig für TextMeshPro

public class QuizManager : MonoBehaviour
{
    public int currentQuestion = 1;
    public int totalQuestions = 5;
    public TextMeshProUGUI progressText;

    void Start()
    {
        UpdateUI();
    }

    public void NextQuestion()
    {
        if (currentQuestion < totalQuestions)
        {
            currentQuestion++;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        progressText.text = "Frage " + currentQuestion + " / " + totalQuestions;
    }
}