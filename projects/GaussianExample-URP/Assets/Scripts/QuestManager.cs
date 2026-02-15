using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("UI Einstellungen")]
    public TextMeshProUGUI questText; 
    
    [Header("Quest Einstellungen")]
    public int zielAnzahl = 3; // Wie viele Buttons insgesamt?
    private int aktuellerStand = 0;

    void Start()
    {
        UpdateUI(); // Zeigt am Anfang 0/3 an
    }

    public void PunktHinzufuegen()
    {
        aktuellerStand++;
        UpdateUI();

        if (aktuellerStand >= zielAnzahl)
        {
            questText.text = "Quest abgeschlossen!";
            questText.color = Color.green;
        }
    }

    void UpdateUI()
    {
        questText.text = "Aufgabe: " + aktuellerStand + " / " + zielAnzahl;
    }
}