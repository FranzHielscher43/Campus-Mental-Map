using UnityEngine;
using UnityEngine.EventSystems; 
public class CanvasObjectInteractionPlane : MonoBehaviour, IPointerClickHandler
{
    [Header("Zuweisung")]
    public GameObject infoCanvas; 

    void Start()
    {
        if (infoCanvas != null) 
        {
            infoCanvas.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (infoCanvas != null)
        {
            bool currentState = infoCanvas.activeSelf;
            infoCanvas.SetActive(!currentState);

            Debug.Log($"<color=cyan>Interaktion:</color> InfoCanvas am {gameObject.name} ist jetzt {(infoCanvas.activeSelf ? "Sichtbar" : "Versteckt")}");
        }
    }
}
