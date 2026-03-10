using UnityEngine;


public class PositionMap : MonoBehaviour
{
    public GameObject pin;

    private bool isVisible = false;

    void Awake()
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        interactable.selectEntered.AddListener(_ => Toggle());
    }

    void Toggle()
    {
        isVisible = !isVisible;
        pin.SetActive(isVisible);
    }
}