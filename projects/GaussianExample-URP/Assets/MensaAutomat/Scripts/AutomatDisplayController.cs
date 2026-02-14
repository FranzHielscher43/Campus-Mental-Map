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
    public TextMeshProUGUI balanceText;        // Feld im BalanceScreen
    public TextMeshProUGUI approveBalanceText; // Feld im ApproveScreen (der neue Wert)
    public TextMeshProUGUI successBalanceText; // Feld im SuccessScreen (der finale Wert)
    public TextMeshProUGUI newBalanceText;   // NEU: Nur der Zielbetrag im ApproveScreen

    private float currentBalance = 0f;
    private float insertedMoney = 0f;

    // --- WORKFLOW: KARTE ---
    public void OnCardInserted()
    {
        StartCoroutine(CardLoadingSequence());
    }

    private IEnumerator CardLoadingSequence()
    {
        ShowPanel(loadingPanel);
        yield return new WaitForSeconds(2.0f);

        string balanceString = currentBalance.ToString("F2") + " €";
        if (balanceText != null) balanceText.text = balanceString;

        ShowPanel(balancePanel);
    }

    public void OnCardEjectRequested()
    {
        ShowPanel(idlePanel);
    }

    // --- WORKFLOW: GELD ---
    // Diese Version nutzt die Dynamic-Event-Anpassung für den XR Socket
    public void OnMoneySocketEntered(SelectEnterEventArgs args)
    {
        GameObject insertedObject = args.interactableObject.transform.gameObject;
        OnMoneyInserted(insertedObject);
    }

    public void OnMoneyInserted(GameObject insertedObject)
    {
        MoneyNote note = insertedObject.GetComponent<MoneyNote>();
        if (note != null)
        {
            insertedMoney = note.value;
            float newTotal = currentBalance + insertedMoney;

            // Hier wird das Feld im ApproveScreen gefüllt
            if (approveBalanceText != null) approveBalanceText.text = currentBalance.ToString("F2") + " €";
            if (newBalanceText != null) newBalanceText.text = newTotal.ToString("F2") + " €";

            ShowPanel(topUpPanel);
            Destroy(insertedObject, 0.5f);
        }
    }

    public void ConfirmTopUp()
    {
        StartCoroutine(TopUpSequence());
    }

    private IEnumerator TopUpSequence()
    {
        ShowPanel(loadingPanel);
        yield return new WaitForSeconds(1.5f);
        currentBalance += insertedMoney;

        string finalBalance = currentBalance.ToString("F2") + " €";

        // Alle relevanten Felder aktualisieren
        if (balanceText != null) balanceText.text = finalBalance;
        if (successBalanceText != null) successBalanceText.text = finalBalance;

        ShowPanel(successPanel);
        yield return new WaitForSeconds(3.0f);
        OnCardEjectRequested();
    }

    private void ShowPanel(GameObject activePanel)
    {
        idlePanel.SetActive(activePanel == idlePanel);
        loadingPanel.SetActive(activePanel == loadingPanel);
        balancePanel.SetActive(activePanel == balancePanel);
        topUpPanel.SetActive(activePanel == topUpPanel);
        successPanel.SetActive(activePanel == successPanel);
    }
}