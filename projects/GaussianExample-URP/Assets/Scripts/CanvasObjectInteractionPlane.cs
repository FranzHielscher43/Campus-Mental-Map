using UnityEngine;
using UnityEngine.EventSystems; // WICHTIG für Klick-Erkennung

public class CanvasObjectInteractionPlane : MonoBehaviour, IPointerClickHandler
{
    [Header("Zuweisung")]
    public GameObject infoCanvas; // Ziehe hier das "InfoCanvas" aus der Hierarchy rein

    void Start()
    {
        // Sicherstellen, dass das Info-Panel am Anfang unsichtbar ist
        if (infoCanvas != null) 
        {
            infoCanvas.SetActive(false);
        }
    }

    // Diese Funktion wird automatisch aufgerufen, wenn man das Objekt anklickt
    public void OnPointerClick(PointerEventData eventData)
    {
        if (infoCanvas != null)
        {
            // Umschalten: Wenn an -> aus, wenn aus -> an
            bool currentState = infoCanvas.activeSelf;
            infoCanvas.SetActive(!currentState);

            Debug.Log($"<color=cyan>Interaktion:</color> InfoCanvas am {gameObject.name} ist jetzt {(infoCanvas.activeSelf ? "Sichtbar" : "Versteckt")}");
        }
    }
}
