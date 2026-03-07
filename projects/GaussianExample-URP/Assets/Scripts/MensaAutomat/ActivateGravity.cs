using UnityEngine;

public class FloatAndDrop : MonoBehaviour
{
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void OnGrab()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}