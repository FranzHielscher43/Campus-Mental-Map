using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem; 
using System.Collections;

public class UniversalInteraction : MonoBehaviour
{
    [Header("Ziel-Objekt")]
    public GameObject targetPlane; 

    [Header("Feste Größe der Plane")]
    public Vector3 fixedScale = new Vector3(0.1f, 0.1f, 0.1f); // Hier die Größe einstellen

    [Header("Slide Einstellungen")]
    public float slideHeight = 1.0f;    
    public float slideDuration = 0.5f; 
    public float forwardOffset = 0.4f; 

    private XRSimpleInteractable xrInteractable;
    private bool isOpen = false;
    private Vector3 closedPos;
    private Vector3 openPos;
    private Coroutine activeSlide;

    void Start()
    {
        xrInteractable = GetComponent<XRSimpleInteractable>();
        if (xrInteractable == null) xrInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        xrInteractable.selectEntered.AddListener((args) => ToggleObject("VR-Controller"));

        if (targetPlane != null)
        {
            // FIX: Wir setzen die Größe fest, unabhängig vom Würfel
            // Da die Plane ein Child ist, berechnen wir die Größe relativ zum Parent
            FixScale();
            
            closedPos = Vector3.zero; 
            openPos = new Vector3(0, slideHeight, forwardOffset);
            
            targetPlane.transform.localPosition = closedPos;
            targetPlane.SetActive(false);
        }
    }

    // Diese Funktion sorgt dafür, dass die Plane trotz Skalierung des Würfels normal aussieht
    void FixScale()
    {
        if (targetPlane != null)
        {
            // Wir teilen die gewünschte Größe durch die Größe des Würfels
            Vector3 parentScale = transform.lossyScale;
            targetPlane.transform.localScale = new Vector3(
                fixedScale.x / parentScale.x,
                fixedScale.y / parentScale.y,
                fixedScale.z / parentScale.z
            );
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == this.transform)
                {
                    ToggleObject("Maus");
                }
            }
        }
    }

    public void ToggleObject(string quelle)
    {
        if (targetPlane == null) return;
        
        // Jedes Mal beim Öffnen die Größe kurz prüfen, falls der Würfel verschoben wurde
        FixScale();

        isOpen = !isOpen;
        if (activeSlide != null) StopCoroutine(activeSlide);
        activeSlide = StartCoroutine(SlideRoutine(isOpen));
    }

    IEnumerator SlideRoutine(bool show)
    {
        if (show) targetPlane.SetActive(true);

        float elapsed = 0;
        Vector3 startPos = targetPlane.transform.localPosition;
        Vector3 endPos = show ? openPos : closedPos;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            targetPlane.transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        targetPlane.transform.localPosition = endPos;
        if (!show) targetPlane.SetActive(false);
    }
}