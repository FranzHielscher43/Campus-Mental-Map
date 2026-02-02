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
    public GameObject hintPanel;              // "Hinsetzen"
    public GameObject standPanel;             // "Aufstehen"
    public GameObject teleportPanel;          // "Teleport"
    public GameObject teleportConfirmButton;  // "Bestätigen"

    [Header("UI Groups (CanvasGroup!)")]
    [SerializeField] private CanvasGroup standGroup;
    [SerializeField] private CanvasGroup teleportGroup;

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
        // Bestätigung läuft ab
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
        // Teleport ist nur erlaubt, wenn man sitzt
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

        // Jetzt Scene laden (statt MoveRigTo)
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

        teleportArmed = false;
        if (teleportConfirmButton) teleportConfirmButton.SetActive(false);

        DisableGroups();
        EventSystem.current?.SetSelectedGameObject(null);

        if (hintPanel) hintPanel.SetActive(false);
        if (standPanel) standPanel.SetActive(false);
        if (teleportPanel) teleportPanel.SetActive(false);

        // Fade to black
        if (fader != null) yield return fader.FadeTo(1f);

        // Kleiner Ghost-Click-Block bevor wir die Szene wechseln
        yield return new WaitForSecondsRealtime(postActionInputBlock);

        // Safety: Scene muss existieren (optional Log)
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[SeatInteractable] teleportSceneName ist leer!");
            if (fader != null) yield return fader.FadeTo(0f);
            busy = false;
            yield break;
        }

        // Scene laden
        SceneManager.LoadScene(sceneName);
        // Hinweis: ab hier läuft das Script nur weiter, wenn es in der neuen Scene noch existiert.
        // Meistens hast du in der neuen Scene auch einen Fader, der beim Start wieder einblendet.

        busy = false;
    }

    // --------------------------------------------------------
    // Helpers
    // --------------------------------------------------------

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