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
    public MeshRenderer planeRenderer; // Der Renderer der Plane (für das Bild)
    public TextMeshPro infoText; 

    [Header("Option A: Bild (Priorität)")]
    public Texture2D screenshot; 
    public float baseImageHeight = 0.1f; // Referenzgröße für Bilder

    [Header("Option B: Text (Fallbacks)")]
    [TextArea(3,10)]
    public string message = "Dies ist ein Beispieltext...";
    public float typeSpeed = 0.05f;

    [Header("Feste Größe (nur für Text)")]
    public Vector3 fixedScale = new Vector3(0.1f, 1f, 0.1f);

    [Header("Animation")]
    public float slideHeight = 1.0f;    
    public float slideDuration = 0.5f; 

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
            PrepareContent(); // Inhalt beim Start prüfen
            closedPos = Vector3.zero; 
            openPos = new Vector3(0, slideHeight, 0.4f);
            targetPlane.transform.localPosition = closedPos;
            targetPlane.SetActive(false);
        }
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
        
        PrepareContent(); // Skalierung/Inhalt vor dem Slide aktualisieren
        isOpen = !isOpen;

        if (activeSlide != null) StopCoroutine(activeSlide);
        if (activeTypewriter != null) StopCoroutine(activeTypewriter);
        
        activeSlide = StartCoroutine(SlideRoutine(isOpen));
    }

    void PrepareContent()
    {
        if (screenshot != null)
        {
            // BILD-LOGIK
            if (infoText != null) infoText.text = ""; 
            
            // Textur zuweisen
            planeRenderer.material.mainTexture = screenshot;
            
            // WICHTIG: Farbe auf Weiß setzen, damit das Bild nicht blau getönt wird
            planeRenderer.material.color = Color.white; 
            if (planeRenderer.material.HasProperty("_BaseColor")) 
                planeRenderer.material.SetColor("_BaseColor", Color.white);

            // Emission ausschalten (falls vorhanden), damit das Bild nicht überstrahlt
            if (planeRenderer.material.HasProperty("_EmissionColor"))
                planeRenderer.material.SetColor("_EmissionColor", Color.black);

            // Seitenverhältnis berechnen
            float aspectRatio = (float)screenshot.width / screenshot.height;
            Vector3 parentScale = transform.lossyScale;

            targetPlane.transform.localScale = new Vector3(
                (baseImageHeight * aspectRatio) / parentScale.x,
                1f / parentScale.y,
                baseImageHeight / parentScale.z
            );
        }
        else
        {
            // TEXT-LOGIK
            // Hier kannst du die Farbe wieder auf Blau setzen, wenn du willst:
            // planeRenderer.material.color = new Color(0, 0.5f, 1f, 0.5f); 

            Vector3 parentScale = transform.lossyScale;
            targetPlane.transform.localScale = new Vector3(
                fixedScale.x / parentScale.x, 
                fixedScale.y / parentScale.y, 
                fixedScale.z / parentScale.z
            );

            if (infoText != null)
            {
                infoText.transform.localScale = new Vector3(
                    1f / targetPlane.transform.lossyScale.x, 
                    1f / targetPlane.transform.lossyScale.y, 
                    1f / targetPlane.transform.lossyScale.z
                ) * 0.1f; 
            }
        }
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
            targetPlane.transform.localPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, elapsed / slideDuration));
            yield return null;
        }

        targetPlane.transform.localPosition = endPos;

        // Nur wenn KEIN Bild da ist, Text schreiben
        if (show && screenshot == null && infoText != null)
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
}