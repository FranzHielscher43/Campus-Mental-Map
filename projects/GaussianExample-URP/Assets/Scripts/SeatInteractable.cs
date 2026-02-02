using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;


public class SeatInteractable : MonoBehaviour
{
    [Header("Rig")]
    public Transform xrOrigin;
    public ScreenFader fader;

    [Header("Seat Points")]
    public Transform seatPoint;
    public Transform standPoint;

    [Header("Teleport Target (Scene)")]
    [Tooltip("Name der Scene, die beim Teleport geladen werden soll (muss in Build Settings sein).")]
    public string teleportSceneName = "Mocap_Labor_Pult";

    [Header("UI Panels")]
    public GameObject hintPanel;              
    public GameObject standPanel;             
    public GameObject teleportPanel;          
    public GameObject teleportConfirmButton;  

    [Header("UI Groups (CanvasGroup!)")]
    [SerializeField] private CanvasGroup standGroup;
    [SerializeField] private CanvasGroup teleportGroup;

    [Header("Locomotion (disable while sitting)")]
    [Tooltip("Move / Turn / Teleport Provider")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider[] locomotionToDisable;

    [Tooltip("Ray Interactors, Line Visuals, etc.")]
    [SerializeField] private Behaviour[] behavioursToDisable;

    [Header("Anti Ghost-Click")]
    [SerializeField] private float postActionInputBlock = 0.3f;

    [Header("Teleport Confirm")]
    [SerializeField] private float confirmWindow = 3f;

    bool inRange;
    bool isSitting;
    bool busy;

    bool teleportArmed;
    float teleportArmedUntil;

    void Start()
    {
        if (hintPanel) hintPanel.SetActive(false);
        if (standPanel) standPanel.SetActive(false);
        if (teleportPanel) teleportPanel.SetActive(false);
        if (teleportConfirmButton) teleportConfirmButton.SetActive(false);
    }

    void Update()
    {
        if (teleportArmed && Time.unscaledTime > teleportArmedUntil)
        {
            teleportArmed = false;
            if (teleportConfirmButton) teleportConfirmButton.SetActive(false);
        }
    }

    // --------------------------------------------------------
    // Trigger
    // --------------------------------------------------------

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        inRange = true;

        if (!isSitting && hintPanel)
            hintPanel.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        inRange = false;

        if (!isSitting && hintPanel)
            hintPanel.SetActive(false);
    }

    bool IsPlayer(Collider other)
    {
        return other.GetComponentInParent<XROrigin>() != null;
    }

    // --------------------------------------------------------
    // Button Callbacks
    // --------------------------------------------------------

    public void SitDown()
    {
        if (!inRange || isSitting || busy) return;
        StartCoroutine(SitRoutine());
    }

    public void StandUp()
    {
        if (!isSitting || busy) return;
        StartCoroutine(StandRoutine());
    }

    public void Teleport()
    {
        if (!isSitting || busy) return;

        teleportArmed = true;
        teleportArmedUntil = Time.unscaledTime + confirmWindow;

        if (teleportConfirmButton)
            teleportConfirmButton.SetActive(true);

        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void TeleportConfirm()
    {
        if (!isSitting || busy) return;
        if (!teleportArmed || Time.unscaledTime > teleportArmedUntil) return;

        teleportArmed = false;
        if (teleportConfirmButton)
            teleportConfirmButton.SetActive(false);

        StartCoroutine(TeleportToSceneRoutine(teleportSceneName));
    }

    // --------------------------------------------------------
    // Routines
    // --------------------------------------------------------

    IEnumerator SitRoutine()
    {
        busy = true;

        if (hintPanel) hintPanel.SetActive(false);

        if (fader != null) yield return fader.FadeTo(1f);

        MoveRigTo(seatPoint);

        // ✅ Bewegung sperren – Umschauen bleibt
        SetSittingLock(true);

        isSitting = true;

        if (standPanel) standPanel.SetActive(true);
        if (teleportPanel) teleportPanel.SetActive(true);

        EventSystem.current?.SetSelectedGameObject(null);

        DisableGroups();

        if (fader != null) yield return fader.FadeTo(0f);

        yield return new WaitForSecondsRealtime(postActionInputBlock);

        EnableGroups();

        busy = false;
    }

    IEnumerator StandRoutine()
    {
        busy = true;

        teleportArmed = false;
        if (teleportConfirmButton) teleportConfirmButton.SetActive(false);

        DisableGroups();
        EventSystem.current?.SetSelectedGameObject(null);

        if (standPanel) standPanel.SetActive(false);
        if (teleportPanel) teleportPanel.SetActive(false);

        if (fader != null) yield return fader.FadeTo(1f);

        MoveRigTo(standPoint);

        // ✅ Bewegung wieder erlauben
        SetSittingLock(false);

        isSitting = false;

        if (inRange && hintPanel)
            hintPanel.SetActive(true);

        if (fader != null) yield return fader.FadeTo(0f);

        yield return new WaitForSecondsRealtime(postActionInputBlock);

        busy = false;
    }

    IEnumerator TeleportToSceneRoutine(string sceneName)
    {
        busy = true;

        DisableGroups();
        SetSittingLock(true);

        if (fader != null) yield return fader.FadeTo(1f);
        yield return new WaitForSecondsRealtime(postActionInputBlock);

        SceneManager.LoadScene(sceneName);
    }

    // --------------------------------------------------------
    // Helpers
    // --------------------------------------------------------

    void SetSittingLock(bool locked)
    {
        if (locomotionToDisable != null)
        {
            foreach (var lp in locomotionToDisable)
                if (lp) lp.enabled = !locked;
        }

        if (behavioursToDisable != null)
        {
            foreach (var b in behavioursToDisable)
                if (b) b.enabled = !locked;
        }
    }

    void DisableGroups()
    {
        if (standGroup)
        {
            standGroup.interactable = false;
            standGroup.blocksRaycasts = false;
        }

        if (teleportGroup)
        {
            teleportGroup.interactable = false;
            teleportGroup.blocksRaycasts = false;
        }
    }

    void EnableGroups()
    {
        if (standGroup)
        {
            standGroup.interactable = true;
            standGroup.blocksRaycasts = true;
        }

        if (teleportGroup)
        {
            teleportGroup.interactable = true;
            teleportGroup.blocksRaycasts = true;
        }
    }

    void MoveRigTo(Transform target)
    {
        if (!xrOrigin || !target) return;

        var origin = xrOrigin.GetComponent<XROrigin>();
        Transform cam = origin != null ? origin.Camera.transform : Camera.main.transform;

        Vector3 headToOrigin = xrOrigin.position - cam.position;
        xrOrigin.position = target.position + headToOrigin;

        xrOrigin.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
    }
}