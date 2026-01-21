using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class SeatInteractable : MonoBehaviour
{
    [Header("Rig")]
    public Transform xrOrigin;
    public ScreenFader fader;

    [Header("Seat Points")]
    public Transform seatPoint;
    public Transform standPoint;
    public Transform exitPoint;

    [Header("UI")]
    public GameObject hintPanel; 
    public GameObject teleportPanel;  
    public GameObject standPanel; 

    bool inRange;
    bool isSitting;
    bool busy;

    void Start()
    {
        if (hintPanel) hintPanel.SetActive(false);
        if (teleportPanel) teleportPanel.SetActive(false);
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

    public void Teleport() 
    {
        if(!isSitting || busy || exitPoint == null) return;
        StartCoroutine(TeleportRoutine(exitPoint));
    }

    IEnumerator SitRoutine()
    {
        busy = true;
        if (hintPanel) hintPanel.SetActive(false);

        if (fader != null) yield return fader.FadeTo(1f);

        MoveRigTo(seatPoint);

        isSitting = true;
        if (standPanel) standPanel.SetActive(true);
        if (teleportPanel) teleportPanel.SetActive(true);

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

    IEnumerator TeleportRoutine(Transform target) 
    {
        busy = true;

        if(hintPanel) hintPanel.SetActive(false);
        if(standPanel) standPanel.SetActive(false);
        if (fader != null) yield return fader.FadeTo(1f);

        MoveRigTo(exitPoint);
        isSitting = false;
        inRange = false;

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