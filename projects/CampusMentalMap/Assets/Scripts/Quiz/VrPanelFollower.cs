using UnityEngine;

public class VRPanelFollower : MonoBehaviour
{
    public float distanceFromHead = 1.5f;
    public float smoothSpeed = 5.0f;
    
    private Transform headCamera;

    void OnEnable()
    {
        if (Camera.main != null)
        {
            headCamera = Camera.main.transform;
            Vector3 targetPos = headCamera.position + (headCamera.forward * distanceFromHead);
            transform.position = targetPos;
            transform.rotation = Quaternion.LookRotation(transform.position - headCamera.position);
        }
    }

    void Update()
    {
        if (headCamera == null) return;

        Vector3 targetPosition = headCamera.position + (headCamera.forward * distanceFromHead);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

        transform.rotation = Quaternion.LookRotation(transform.position - headCamera.position);
    }
}