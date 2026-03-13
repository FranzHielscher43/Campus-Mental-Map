using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("UI Einstellungen")]
    public TextMeshProUGUI questText; 
    
    [Header("Quest Einstellungen")]
    public int zielAnzahl = 3; 
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
            Color finishColor;
            if(ColorUtility.TryParseHtmlString("#1CA0DA", out finishColor))
            {
                questText.color = finishColor;
            }
        }
    }

    void UpdateUI()
    {
        questText.text = "Aufgabe: " + aktuellerStand + " / " + zielAnzahl;
    }
}