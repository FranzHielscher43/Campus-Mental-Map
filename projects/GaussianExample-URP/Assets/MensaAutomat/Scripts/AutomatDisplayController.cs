using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class MensaManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject idlePanel;
    public GameObject loadingPanel;
    public GameObject balancePanel;
    public GameObject topUpPanel;
    public GameObject successPanel;

    [Header("Text Fields")]
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI topUpInfoText;

    private float currentBalance = 12.50f; // Beispielwert
    private float insertedMoney = 0f;

    // --- WORKFLOW: KARTE ---

    public void OnCardInserted()
    {
        StartCoroutine(CardLoadingSequence());
    }

    private IEnumerator CardLoadingSequence()
    {
        ShowPanel(loadingPanel);
        yield return new WaitForSeconds(2.0f); // Ladezeit
        balanceText.text = currentBalance.ToString("F2") + " €";
        ShowPanel(balancePanel);
    }

    public void OnCardEjectRequested()
    {
        // Hier Befehl an den Socket senden: Eject()
        ShowPanel(idlePanel);
    }

    // --- WORKFLOW: GELD ---

    public void OnMoneyInserted(GameObject insertedObject)
    {
        MoneyNote note = insertedObject.GetComponent<MoneyNote>();

        if (note != null)
        {
            insertedMoney = note.value;
            float newTotal = currentBalance + insertedMoney;

            topUpInfoText.text = $"Alt: {currentBalance:F2}€\nNeu: {newTotal:F2}€";
            ShowPanel(topUpPanel);

            // Optional: Zerstöre den Geldschein (er wird "eingezogen")
            Destroy(insertedObject, 0.5f);
        }
        else
        {
            Debug.LogWarning("Objekt im Geld-Slot hat keine MoneyNote-Komponente!");
        }
    }

    public void ConfirmTopUp()
    {
        StartCoroutine(TopUpSequence());
    }

    public void OnMoneySocketEntered(SelectEnterEventArgs args)
    {
        // Wir holen uns das Objekt, das gerade in den Socket gesteckt wurde
        GameObject insertedObject = args.interactableObject.transform.gameObject;

        // Wir rufen deine vorhandene Funktion auf
        OnMoneyInserted(insertedObject);
    }

    private IEnumerator TopUpSequence()
    {
        ShowPanel(loadingPanel);
        yield return new WaitForSeconds(1.5f);
        currentBalance += insertedMoney;
        balanceText.text = currentBalance.ToString("F2") + " €";
        ShowPanel(successPanel);

        // Automatisch Karte auswerfen nach 3 Sekunden Erfolg
        yield return new WaitForSeconds(3.0f);
        OnCardEjectRequested();
    }

    // Hilfsfunktion zum Umschalten
    private void ShowPanel(GameObject activePanel)
    {
        idlePanel.SetActive(activePanel == idlePanel);
        loadingPanel.SetActive(activePanel == loadingPanel);
        balancePanel.SetActive(activePanel == balancePanel);
        topUpPanel.SetActive(activePanel == topUpPanel);
        successPanel.SetActive(activePanel == successPanel);
    }
}