using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections;

[RequireComponent(typeof(XRSimpleInteractable))]
public class UniversalInteractionVR : MonoBehaviour
{
    [Header("Ziel-Objekte")]
    public GameObject targetPlane; 
    public MeshRenderer planeRenderer; 
    public TextMeshPro infoText; 

    [Header("Inhalt")]
    public Texture2D screenshot; 
    public float baseImageHeight = 0.1f; 
    
    [Tooltip("Text hier eintippen, wenn das TextMeshPro Feld leer ist.")]
    [TextArea(3,10)]
    public string fallbackMessage = "Fallback Text..."; 
    public float typeSpeed = 0.05f;

    [Header("Größen & Layout (Hier reparierst du den Overflow!)")]
    public Vector3 fixedScale = new Vector3(0.1f, 1f, 0.1f);
    // NEU: Damit kannst du die Text-Breite manuell anpassen
    public Vector2 textAreaSize = new Vector2(5.0f, 5.0f); 
    public float slideHeight = 0.5f;    
    public float slideDuration = 0.5f; 
    
    [Header("Hover Feedback")]
    public Color hoverColor = Color.cyan;
    private Color originalObjectColor;
    private Material objectMaterial;

    private XRSimpleInteractable xrInteractable;
    private bool isOpen = false;
    private Vector3 closedPos;
    private Vector3 openPos;
    private Coroutine activeSlide;
    private Coroutine activeTypewriter;
    private string finalContentText; 

    private float lastClickTime = 0f;
    private float clickCooldown = 0.5f;

    void Start()
    {
        xrInteractable = GetComponent<XRSimpleInteractable>();
        xrInteractable.selectEntered.AddListener(OnSelect);
        xrInteractable.hoverEntered.AddListener(OnHoverEnter);
        xrInteractable.hoverExited.AddListener(OnHoverExit);

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            objectMaterial = rend.material;
            originalObjectColor = objectMaterial.color;
        }

        if (infoText != null)
        {
            if (!string.IsNullOrWhiteSpace(infoText.text) && infoText.text != "New Text")
                finalContentText = infoText.text;
            else
                finalContentText = fallbackMessage;
            
            infoText.text = ""; 
        }

        if (targetPlane != null)
        {
            PrepareContent(); 
            closedPos = Vector3.zero; 
            openPos = new Vector3(0, slideHeight, 0); 
            targetPlane.transform.localPosition = closedPos;
            targetPlane.SetActive(false);
        }
    }

    void Update()
    {
        if (isOpen && targetPlane != null && Camera.main != null)
        {
            RotatePanelToPlayer();
        }
        
        // DEBUG-HILFE: Erlaubt dir, die Größe LIVE im Play-Mode zu ändern!
        if (isOpen && infoText != null && screenshot == null)
        {
             RectTransform textRect = infoText.GetComponent<RectTransform>();
             if (textRect.sizeDelta != textAreaSize)
             {
                 textRect.sizeDelta = textAreaSize;
             }
        }
    }

    private void OnSelect(SelectEnterEventArgs args) => ToggleObject();
    
    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (objectMaterial != null) objectMaterial.color = hoverColor;
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (objectMaterial != null) objectMaterial.color = originalObjectColor;
    }

    public void ToggleObject()
    {
        if (Time.time - lastClickTime < clickCooldown) return;
        lastClickTime = Time.time;

        if (targetPlane == null) return;
        
        PrepareContent(); 
        isOpen = !isOpen;

        if (activeSlide != null) StopCoroutine(activeSlide);
        if (activeTypewriter != null) StopCoroutine(activeTypewriter);
        
        activeSlide = StartCoroutine(SlideRoutine(isOpen));
    }

    private void RotatePanelToPlayer()
    {
        Vector3 direction = Camera.main.transform.position - targetPlane.transform.position;
        direction.y = 0; 
        if (direction != Vector3.zero)
        {
            targetPlane.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    void PrepareContent()
    {
        if (screenshot != null)
        {
            if (infoText != null) infoText.text = ""; 
            planeRenderer.material.mainTexture = screenshot;
            planeRenderer.material.color = Color.white; 
            if (planeRenderer.material.HasProperty("_BaseColor")) 
                planeRenderer.material.SetColor("_BaseColor", Color.white);

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
            Vector3 parentScale = transform.lossyScale;
            targetPlane.transform.localScale = new Vector3(
                fixedScale.x / parentScale.x, 
                fixedScale.y / parentScale.y, 
                fixedScale.z / parentScale.z
            );

            if (infoText != null)
            {
                infoText.transform.localScale = new Vector3(
                    0.2f / targetPlane.transform.localScale.x, 
                    0.2f / targetPlane.transform.localScale.y, 
                    0.2f / targetPlane.transform.localScale.z
                );

                RectTransform textRect = infoText.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    // HIER WIRD DEIN WERT GENUTZT
                    textRect.sizeDelta = textAreaSize; 
                }
                
                infoText.enableWordWrapping = true;
                infoText.alignment = TextAlignmentOptions.Center;
                infoText.enableAutoSizing = true;
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

        if (show && screenshot == null && infoText != null)
        {
            activeTypewriter = StartCoroutine(TypeWriterRoutine());
        }

        if (!show) targetPlane.SetActive(false);
    }

    IEnumerator TypeWriterRoutine()
    {
        infoText.text = "";
        foreach (char c in finalContentText.ToCharArray()) 
        {
            infoText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }
}
