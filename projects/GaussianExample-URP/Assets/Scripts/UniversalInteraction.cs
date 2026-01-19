using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.InputSystem; 
using TMPro;
using System.Collections;

public class UniversalInteraction : MonoBehaviour
{
    [Header("Ziel-Objekte")]
    public GameObject targetPlane; 
    public TextMeshPro infoText; // Hier das PlaneText Objekt reinziehen

    [Header("Inhalt")]
    [TextArea(3,10)]
    public string message = "Dies ist ein Beispieltext für die Anzeige auf der Plane. Hallo Welt! wie geht's dir heute? gut und du? hallo hallo";
    [Header("Feste Größe der Plane")]
    public Vector3 fixedScale = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Animation")]
    public float slideHeight = 1.0f;    
    public float slideDuration = 0.5f; 
    public float typeSpeed = 0.05f; // Geschwindigkeit des Text-Aufbaus

    private XRSimpleInteractable xrInteractable;
    private bool isOpen = false;
    private Vector3 closedPos;
    private Vector3 openPos;
    private Coroutine activeSlide;
    private Coroutine activeTypewriter;

    void Start()
    {
        xrInteractable = GetComponent<XRSimpleInteractable>() ?? gameObject.AddComponent<XRSimpleInteractable>();
        xrInteractable.selectEntered.AddListener((args) => ToggleObject("VR-Controller"));

        if (targetPlane != null)
        {
            FixScale();
            closedPos = Vector3.zero; 
            openPos = new Vector3(0, slideHeight, 0.4f);
            targetPlane.transform.localPosition = closedPos;
            targetPlane.SetActive(false);
        }

        if (infoText != null) infoText.text = ""; // Text am Anfang leer
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == this.transform)
            {
                ToggleObject("Maus");
            }
        }
    }

    public void ToggleObject(string quelle)
    {
        if (targetPlane == null) return;
        FixScale();
        isOpen = !isOpen;

        if (activeSlide != null) StopCoroutine(activeSlide);
        if (activeTypewriter != null) StopCoroutine(activeTypewriter);
        
        activeSlide = StartCoroutine(SlideRoutine(isOpen));
    }

    IEnumerator SlideRoutine(bool show)
    {
        if (show) 
        {
            targetPlane.SetActive(true);
            if (infoText != null) infoText.text = ""; // Text leeren vor neuem Slide
        }

        float elapsed = 0;
        Vector3 startPos = targetPlane.transform.localPosition;
        Vector3 endPos = show ? openPos : closedPos;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            targetPlane.transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, elapsed / slideDuration));
            yield return null;
        }

        targetPlane.transform.localPosition = endPos;

        // Wenn offen: Starte den Schreibmaschinen-Effekt
        if (show && infoText != null)
        {
            activeTypewriter = StartCoroutine(TypeWriterRoutine());
        }

        if (!show) targetPlane.SetActive(false);
    }

    IEnumerator TypeWriterRoutine()
    {
        infoText.text = "";
        foreach (char c in message.ToCharArray())
        {
            infoText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    void FixScale()
{
    if (targetPlane != null)
    {
        Vector3 parentScale = transform.lossyScale;
        // Plane skalieren
        targetPlane.transform.localScale = new Vector3(fixedScale.x / parentScale.x, fixedScale.y / parentScale.y, fixedScale.z / parentScale.z);
        
        if (infoText != null)
        {
            infoText.transform.localScale = new Vector3(1f / targetPlane.transform.lossyScale.x, 1f / targetPlane.transform.lossyScale.y, 1f / targetPlane.transform.lossyScale.z) * 0.1f; 
        }
    }
}
}