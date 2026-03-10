using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class RotateWhileGrabbed : MonoBehaviour
{
    [Header("Assign the visual child you want to rotate (e.g. Model)")]
    public Transform modelToRotate;

    [Header("Input (Right Controller)")]
    [Tooltip("Quest: A = primaryButton, B = secondaryButton. For B choose Secondary Button.")]
    public InputActionProperty rotateButton;

    public float degreesPerSecond = 100f;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    bool isGrabbed;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void OnEnable()
    {
        grab.selectEntered.AddListener(_ => isGrabbed = true);
        grab.selectExited.AddListener(_ => isGrabbed = false);
        rotateButton.action?.Enable();
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveAllListeners();
        grab.selectExited.RemoveAllListeners();
        rotateButton.action?.Disable();
    }

    void Update()
    {
        if (!isGrabbed) return;
        if (modelToRotate == null) return;
        if (rotateButton.action == null) return;
        if (!rotateButton.action.IsPressed()) return;

        modelToRotate.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime, Space.Self);
    }
}