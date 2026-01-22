using UnityEngine;
using UnityEngine.InputSystem; 
using TMPro;
using System.Collections;

public class StartSceneIntro : MonoBehaviour
{
    [Header("Ziel-Objekte")]
    public GameObject[] targetPlanes; 
    public TextMeshPro[] infoTexts; 
    public MeshRenderer planeRenderer; 

    [Header("Auto-Start")]
    public float autoStartDelay = 3.0f; 

    [Header("Animation")]
    public float slideHeight = 1.0f;    
    public float slideDuration = 0.5f; 
    [Tooltip("Mindestdistanz in Pixeln für eine Wischbewegung")]
    public float swipeThreshold = 50f; 

    [Header("VR Support (Optional)")]
    public InputActionProperty vrClickAction; 
    public Transform vrPointerTransform;    

    private bool isOpen = false;
    private int currentSlideIndex = 0;
    private Vector3 closedPos;
    private Vector3 openPos;
    private Coroutine activeSlide;

    private Vector2 startMousePos;
    private Vector3 startVRDir; 
    private bool hitPlaneOnDown = false;
    private bool usedVRThisFrame = false;

    void Start()
    {
        if (targetPlanes != null && targetPlanes.Length > 0)
        {
            closedPos = Vector3.zero; 
            openPos = new Vector3(0, slideHeight, 0.4f);
            
            for (int i = 0; i < targetPlanes.Length; i++) {
                targetPlanes[i].transform.localPosition = closedPos;
                targetPlanes[i].SetActive(false);
                if (i < infoTexts.Length && infoTexts[i] != null) infoTexts[i].gameObject.SetActive(false);
            }
            StartCoroutine(AutoStartTimer());
        }
    }

    IEnumerator AutoStartTimer()
    {
        yield return new WaitForSeconds(autoStartDelay);
        if (!isOpen) OpenPlane();
    }

    void Update()
    {
        if (!isOpen) return;

        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool vrPressed = vrClickAction.action != null && vrClickAction.action.WasPressedThisFrame();

        if (mousePressed || vrPressed)
        {
            Ray ray;
            if (vrPressed && vrPointerTransform != null)
            {
                ray = new Ray(vrPointerTransform.position, vrPointerTransform.forward);
                startVRDir = vrPointerTransform.forward;
                usedVRThisFrame = true;
            }
            else if (Mouse.current != null)
            {
                startMousePos = Mouse.current.position.ReadValue();
                ray = Camera.main.ScreenPointToRay(startMousePos);
                usedVRThisFrame = false;
            }
            else return;

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject current = targetPlanes[currentSlideIndex];
                hitPlaneOnDown = (hit.transform == current.transform || hit.transform.IsChildOf(current.transform));
            }
            else hitPlaneOnDown = false;
        }

        bool mouseReleased = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
        bool vrReleased = vrClickAction.action != null && vrClickAction.action.WasReleasedThisFrame();

        if (mouseReleased || vrReleased)
        {
            float moveValue = 0;
            if (usedVRThisFrame && vrPointerTransform != null)
                moveValue = Vector3.Angle(startVRDir, vrPointerTransform.forward) * 10f; 
            else if (Mouse.current != null)
                moveValue = Vector2.Distance(startMousePos, Mouse.current.position.ReadValue());

            if (moveValue > swipeThreshold)
            {
                if (hitPlaneOnDown) NextSlide(); 
            }
            else
            {
                if (!hitPlaneOnDown) ClosePlane(); 
            }
        }
    }

    public void NextSlide()
    {
        if (!isOpen) return;
        if (currentSlideIndex < targetPlanes.Length - 1) StartCoroutine(NextSlideRoutine());
        else ClosePlane();
    }

    IEnumerator NextSlideRoutine()
    {
        // 1. Die alte Slide runterfahren (ohne 'yield return', also läuft sie im Hintergrund)
        StartCoroutine(SlideRoutine(currentSlideIndex, false));
        
        // Index erhöhen
        currentSlideIndex++;
        
        // 2. Die neue Slide hochfahren (mit 'yield return', damit die Routine hier wartet)
        yield return StartCoroutine(SlideRoutine(currentSlideIndex, true));
    }

    public void OpenPlane()
    {
        if (isOpen) return;
        isOpen = true;
        ToggleAnimation(currentSlideIndex, true);
    }

    public void ClosePlane()
    {
        if (!isOpen) return;
        isOpen = false;
        ToggleAnimation(currentSlideIndex, false);
    }

    private void ToggleAnimation(int index, bool show)
    {
        if (activeSlide != null) StopCoroutine(activeSlide);
        activeSlide = StartCoroutine(SlideRoutine(index, show));
    }

    IEnumerator SlideRoutine(int index, bool show)
    {
        GameObject target = targetPlanes[index];
        GameObject textObj = (index < infoTexts.Length && infoTexts[index] != null) ? infoTexts[index].gameObject : null;
        
        MeshRenderer mr = target.GetComponent<MeshRenderer>();
        if (mr != null) planeRenderer = mr;

        if (show) {
            target.SetActive(true);
            if (textObj != null) textObj.SetActive(true);
        }

        float elapsed = 0;
        Vector3 startPos = target.transform.localPosition;
        Vector3 endPos = show ? openPos : closedPos;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / slideDuration);
            target.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        target.transform.localPosition = endPos;
        if (!show) {
            target.SetActive(false);
            if (textObj != null) textObj.SetActive(false);
        }
    }
}