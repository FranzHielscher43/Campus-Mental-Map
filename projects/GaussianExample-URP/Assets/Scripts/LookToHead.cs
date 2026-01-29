using UnityEngine;

public class LookToHead : MonoBehaviour
{
    public Transform head;
    public bool onlyYaw = true;
    public float yawOffset = 180f; // <- WICHTIG: oft 180

    void LateUpdate()
    {
        if (!head) return;

        Vector3 dir = head.position - transform.position; // Objekt -> Kamera

        if (onlyYaw)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;
        }

        transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(0f, yawOffset, 0f);
    }
}