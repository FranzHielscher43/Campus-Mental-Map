using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("UI Einstellungen")]
    public TextMeshProUGUI questText; 
    
    [Header("Quest Einstellungen")]
    public int zielAnzahl = 7; 
    private int aktuellerStand = 0;

    void Start()
    {
        UpdateUI();
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