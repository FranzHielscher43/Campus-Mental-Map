
using UnityEngine;

public class PlaneScript : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            // Korrektur, da Planen oft flach liegen:
            transform.Rotate(90, 0, 0); 
        }
    }
}
