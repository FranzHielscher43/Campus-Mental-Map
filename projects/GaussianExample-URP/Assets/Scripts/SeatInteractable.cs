using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class SeatInteractable : MonoBehaviour
{
    [Header("Rig")]
    public Transform xrOrigin;     // XR Origin (XR Rig)
    public ScreenFader fader;

    [Header("Seat Points")]
    public Transform seatPoint;
    public Transform standPoint;

    [Header("UI")]
    public GameObject hintPanel;   // Panel mit "Hinsetzen"
    public GameObject standPanel;  // Panel mit "Aufstehen"

    bool inRange;
    bool isSitting;
    bool busy;

    void Start()
    {
        if (hintPanel) hintPanel.SetActive(false);
        if (standPanel) standPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;
        inRange = true;
        if (!isSitting && hintPanel) hintPanel.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;
        inRange = false;
        if (!isSitting && hintPanel) hintPanel.SetActive(false);
    }

    bool IsPlayer(Collider other)
    {
        return other.GetComponentInParent<XROrigin>() != null;
    }

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

    IEnumerator SitRoutine()
    {
        busy = true;
        if (hintPanel) hintPanel.SetActive(false);

        if (fader != null) yield return fader.FadeTo(1f);

        MoveRigTo(seatPoint);

        isSitting = true;
        if (standPanel) standPanel.SetActive(true);

        if (fader != null) yield return fader.FadeTo(0f);

        busy = false;
    }

    IEnumerator StandRoutine()
    {
        busy = true;
        if (standPanel) standPanel.SetActive(false);

        if (fader != null) yield return fader.FadeTo(1f);

        MoveRigTo(standPoint);

        isSitting = false;
        if (inRange && hintPanel) hintPanel.SetActive(true);

        if (fader != null) yield return fader.FadeTo(0f);

        busy = false;
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