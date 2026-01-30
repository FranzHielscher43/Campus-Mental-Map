using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HintLine : MonoBehaviour
{
    [Header("Endpoints")]
    public Transform start;   // z.B. Text / Hint
    public Transform end;     // z.B. Button
    public Transform head;    // XR Camera (wichtig!)

    [Header("Curve Shape")]
    [Tooltip("+1 = nach oben, -1 = nach unten")]
    public float vertical = 1f;

    [Tooltip("+1 = nach außen, -1 = nach innen")]
    public float sideways = 1f;

    [Tooltip("+ = nach vorne, - = nach hinten")]
    public float forward = 0f;

    [Header("Curve Strength")]
    public float bendUp = 0.03f;
    public float bendSide = 0.06f;
    public float bendForward = 0.00f;

    public float bendUpPerMeter = 0.02f;
    public float bendSidePerMeter = 0.03f;
    public float bendForwardPerMeter = 0.00f;

    [Header("Rendering")]
    [Range(8, 64)]
    public int segments = 24;

    [Header("Smoothing")]
    public bool smoothControlPoint = true;
    public float controlFollowSpeed = 12f;

    LineRenderer lr;
    Vector3 smoothP1;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments;
        lr.useWorldSpace = true;
    }

    void OnEnable()
    {
        Application.onBeforeRender += UpdateLine;
    }

    void OnDisable()
    {
        Application.onBeforeRender -= UpdateLine;
    }

    void LateUpdate()
    {
        UpdateLine();
    }

    void UpdateLine()
    {
        if (!start || !end) return;

        Vector3 p0 = start.position;
        Vector3 p2 = end.position;

        float dist = Vector3.Distance(p0, p2);

        // Kopf-Richtungen
        Vector3 headRight = head ? head.right : Vector3.right;
        Vector3 headForward = head
            ? Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized
            : Vector3.forward;

        if (headForward.sqrMagnitude < 0.001f)
            headForward = Vector3.forward;

        Vector3 mid = (p0 + p2) * 0.5f;

        float upAmount   = (bendUp + dist * bendUpPerMeter) * vertical;
        float sideAmount = (bendSide + dist * bendSidePerMeter) * sideways;
        float fwdAmount  = (bendForward + dist * bendForwardPerMeter) * forward;

        Vector3 targetP1 =
            mid
            + Vector3.up * upAmount
            + headRight * sideAmount
            + headForward * fwdAmount;

        if (smoothControlPoint)
            smoothP1 = Vector3.Lerp(smoothP1, targetP1, Time.unscaledDeltaTime * controlFollowSpeed);
        else
            smoothP1 = targetP1;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (segments - 1f);
            Vector3 a = Vector3.Lerp(p0, smoothP1, t);
            Vector3 b = Vector3.Lerp(smoothP1, p2, t);
            lr.SetPosition(i, Vector3.Lerp(a, b, t));
        }
    }
}