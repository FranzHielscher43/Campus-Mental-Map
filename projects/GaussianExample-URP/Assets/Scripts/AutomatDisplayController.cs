using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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


    private void start()
    {
        if (moneySocket != null)
        {
            moneySocket.enabled = false;
        }
    }

    public void OnCardInserted()
    {
        cardSocket.attachTransform = pointInCardSocket;
        SetObjectVisibility(cardSocket, false);
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
            cardSocket.attachTransform = pointOutCardSocket;
            moneySocket.attachTransform = pointOutMoneySocket;
            SetObjectVisibility(cardSocket, true);
            SetObjectVisibility(moneySocket, true);

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
        SetObjectVisibility(cardSocket, true);
        SetObjectVisibility(moneySocket, true);
        insertedMoney = 0;
        currentInsertedNote = null;
        isProcessFinished = true;
    }

    public void OnMoneySocketEntered(SelectEnterEventArgs args)
    {
        GameObject insertedObject = args.interactableObject.transform.gameObject;
        OnMoneyInserted(insertedObject);
    }

    public void OnMoneyInserted(GameObject insertedObject)
    {
        if (cardSocket.attachTransform == pointInCardSocket)
        {
            MoneyNote note = insertedObject.GetComponent<MoneyNote>();
            if (note != null)
            {
                currentInsertedNote = insertedObject;
                insertedMoney = note.value;
                moneySocket.attachTransform = pointInMoneySocket;
                SetObjectVisibility(moneySocket, false);

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
        else
        {
            moneySocket.enabled = false;
        }
    }

    public void CancelTransaction()
    {
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
        SetObjectVisibility(moneySocket, false);
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

    private void SetObjectVisibility(XRSocketInteractor socket, bool isVisible)
    {
        if (socket.hasSelection)
        {
            GameObject rootObject = socket.interactablesSelected[0].transform.gameObject;
            MeshRenderer[] renderers = rootObject.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer ren in renderers)
            {
                ren.enabled = isVisible;
            }
        }
    }
}