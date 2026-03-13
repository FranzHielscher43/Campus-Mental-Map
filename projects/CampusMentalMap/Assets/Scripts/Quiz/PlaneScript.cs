
using UnityEngine;

public class PlaneScript : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(90, 0, 0); 
        }
    }
}
