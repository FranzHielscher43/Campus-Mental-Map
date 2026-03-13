using UnityEngine;

public class LookToHead : MonoBehaviour
{
    public Transform head;
    public float yawOffset = 180f;

    void LateUpdate()
    {
        if (!head) return;

        Vector3 dir = head.position - transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = look * Quaternion.Euler(0f, yawOffset, 0f);
    }
}