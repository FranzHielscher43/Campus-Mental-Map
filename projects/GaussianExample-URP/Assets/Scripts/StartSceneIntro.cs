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

    private bool isOpen = false;
    private int currentSlideIndex = 0;
    private Vector3 closedPos;
    private Vector3 openPos;
    private Coroutine activeSlide;

    // Variablen für die Wisch-Logik
    private Vector2 startMousePos;
    private bool hitPlaneOnDown = false;

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
        if (!isOpen || Mouse.current == null) return;

        // 1. Drücken (Start des Klicks oder Swipes)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            startMousePos = Mouse.current.position.ReadValue();
            
            Ray ray = Camera.main.ScreenPointToRay(startMousePos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject current = targetPlanes[currentSlideIndex];
                // Haben wir die Plane am Anfang berührt?
                hitPlaneOnDown = (hit.transform == current.transform || hit.transform.IsChildOf(current.transform));
            }
            else
            {
                hitPlaneOnDown = false;
            }
        }

        // 2. Loslassen (Check: War es ein Swipe oder nur ein Klick?)
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Vector2 endMousePos = Mouse.current.position.ReadValue();
            float distance = Vector2.Distance(startMousePos, endMousePos);

            if (distance > swipeThreshold)
            {
                // ES WAR EIN WISCHEN
                if (hitPlaneOnDown)
                {
                    NextSlide(); // Auf der Plane gewischt -> Weiter
                }
            }
            else
            {
                // ES WAR EIN KLICK
                if (!hitPlaneOnDown)
                {
                    ClosePlane(); // Außerhalb geklickt -> Runterfahren
                }
            }
        }
    }

    public void NextSlide()
    {
        if (!isOpen) return;

        if (currentSlideIndex < targetPlanes.Length - 1) 
        {
            StartCoroutine(NextSlideRoutine());
        } 
        else 
        {
            ClosePlane();
        }
    }

    IEnumerator NextSlideRoutine()
    {
        yield return StartCoroutine(SlideRoutine(currentSlideIndex, false));
        currentSlideIndex++;
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
        planeRenderer = target.GetComponent<MeshRenderer>();

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