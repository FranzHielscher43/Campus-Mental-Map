using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable))]
public class ObjectQuestTrigger : MonoBehaviour
{
    private bool wurdeSchonGezaehlt = false;
    private QuestManager meinManager;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
    }

    void Start()
    {
        meinManager = FindFirstObjectByType<QuestManager>();
        if (meinManager == null)
            Debug.LogError("Kein QuestManager in der Szene gefunden!");
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        VersuchePunktZuGeben();
    }

    void VersuchePunktZuGeben()
    {
        if (wurdeSchonGezaehlt || meinManager == null) return;

        wurdeSchonGezaehlt = true;
        meinManager.PunktHinzufuegen();
        Debug.Log("Objekt gezählt! Gesamtcounter steigt.");
    }
}