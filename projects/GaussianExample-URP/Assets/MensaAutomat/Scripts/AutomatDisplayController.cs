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
    public Transform pointInCardSocket;
    public Transform pointOutCardSocket;

    [Header("Money Socket Settings")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor moneySocket;
    public Transform pointInMoneySocket;
    public Transform pointOutMoneySocket;

    private GameObject currentInsertedNote;
    private float currentBalance = 0f;
    private float insertedMoney = 0f;
    private bool isProcessFinished = false;

    // --- Initialize ---
    private void start()
    {
        if (moneySocket != null)
        {
            moneySocket.enabled = false;
        }
    }

    // --- WORKFLOW: KARTE ---
    public void OnCardInserted()
    {
        cardSocket.attachTransform = pointInCardSocket;
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

        if (moneySocket != null)
        {
            moneySocket.enabled = true;
        }
    }

    private void CheckAndResetToIdle()
    {
        bool cardIsGone = !cardSocket.hasSelection;
        bool moneyIsGone = !moneySocket.hasSelection;

        if (cardIsGone && moneyIsGone && isProcessFinished)
        {
            isProcessFinished = false;
            cardSocket.attachTransform = pointInCardSocket;
            moneySocket.attachTransform = pointInMoneySocket;

            if (moneySocket != null)
            {
                moneySocket.enabled = false;
            }

            ShowPanel(idleScreen);
        }
    }

    public void OnObjectRemoved()
    {
        CheckAndResetToIdle();
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

        cardSocket.attachTransform = pointOutCardSocket;
        moneySocket.attachTransform = pointOutMoneySocket;
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
            currentInsertedNote = insertedObject;
            insertedMoney = note.value;
            moneySocket.attachTransform = pointInMoneySocket;

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
        }
    }

    public void CancelTransaction()
    {
        if (moneySocket != null)
        {
            moneySocket.enabled = false;
        }
        moneySocket.attachTransform = pointOutMoneySocket;
        insertedMoney = 0;
        currentInsertedNote = null;
        OnCardEjectRequested();
    }

    public void ConfirmApproveScreen()
    {
        if (moneySocket != null)
        {
            moneySocket.enabled = false;
        }
        if (currentInsertedNote != null)
        {
            Destroy(currentInsertedNote, 0.5f);
            currentInsertedNote = null;
        }
        moneySocket.attachTransform = pointInMoneySocket;
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