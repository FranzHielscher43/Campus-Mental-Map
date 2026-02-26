using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class MensaManager : MonoBehaviour
{
    [Header("UI Screens")]
    public GameObject idleScreen;
    public GameObject loadingScreen;
    public GameObject balanceScreen;
    public GameObject ApproveScreen;
    public GameObject successScreen;
    public GameObject ejectScreen;

    [Header("Text Fields")]
    public TextMeshProUGUI balanceScreenBalanceText;
    public TextMeshProUGUI approveScreenOldBalanceText;
    public TextMeshProUGUI approveScreenNewBalanceText;
    public TextMeshProUGUI successScreenBalanceText;

    [Header("Card Socket Settings")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor cardSocket;
    public Transform pointIn;
    public Transform pointOut;

    [Header("Money Socket Settings")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor moneySocket;
    public Transform pointIn;
    public Transform pointOut;

    private float currentBalance = 0f;
    private float insertedMoney = 0f;
    private bool isProcessFinished = false;

    // --- WORKFLOW: KARTE ---
    public void OnCardInserted()
    {
        cardSocket.attachTransform = pointIn;
        isProcessFinished = false;
        StartCoroutine(CardLoadingSequence());
    }

    private IEnumerator CardLoadingSequence()
    {
        ShowPanel(loadingScreen);
        yield return new WaitForSeconds(2.0f);

        string balanceString = currentBalance.ToString("F2") + " €";
        if (balanceScreenBalanceText != null)
        {
            balanceScreenBalanceText.text = balanceString;
        }

        ShowPanel(balanceScreen);
    }

    public void OnCardRemoved()
    {
        if (isProcessFinished)
        {
            isProcessFinished = false;
            cardSocket.attachTransform = pointIn;
            ShowPanel(idleScreen);
        }
    }

    public void OnCardEjectRequested()
    {
        StartCoroutine(OnCardEjectRequestedSequence());
    }

    private IEnumerator OnCardEjectRequestedSequence()
    {
        ShowPanel(loadingScreen);
        yield return new WaitForSeconds(2.0f);
        ShowPanel(ejectScreen);

        cardSocket.attachTransform = pointOut;
        isProcessFinished = true;
    }

    // --- WORKFLOW: GELD ---
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

            if (approveScreenOldBalanceText != null)
            {
                approveScreenOldBalanceText.text = currentBalance.ToString("F2") + " €";
            }
            if (approveScreenNewBalanceText != null)
            {
                approveScreenNewBalanceText.text = newTotal.ToString("F2") + " €";
            }

            ShowPanel(ApproveScreen);
            Destroy(insertedObject, 0.5f);
        }
    }

    public void ConfirmApproveScreen()
    {
        Debug.Log("Button am ApproveScreen wurde gedrückt!");
        StartCoroutine(ConfirmApproveScreenSequence());
    }

    private IEnumerator ConfirmApproveScreenSequence()
    {
        ShowPanel(loadingScreen);
        yield return new WaitForSeconds(2.0f);
        currentBalance += insertedMoney;

        string finalBalance = currentBalance.ToString("F2") + " €";

        // Alle relevanten Felder aktualisieren
        if (balanceScreenBalanceText != null) balanceScreenBalanceText.text = finalBalance;
        if (successScreenBalanceText != null) successScreenBalanceText.text = finalBalance;

        ShowPanel(successScreen);
        yield return new WaitForSeconds(2.0f);
        OnCardEjectRequested();
    }

    private void ShowPanel(GameObject activePanel)
    {
        idleScreen.SetActive(activePanel == idleScreen);
        loadingScreen.SetActive(activePanel == loadingScreen);
        balanceScreen.SetActive(activePanel == balanceScreen);
        ApproveScreen.SetActive(activePanel == ApproveScreen);
        successScreen.SetActive(activePanel == successScreen);
        ejectScreen.SetActive(activePanel == ejectScreen);
    }
}