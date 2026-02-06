using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
// Falls du Unity 6 / Toolkit 3.x nutzt:
using UnityEngine.XR.Interaction.Toolkit.Interactables; 

[RequireComponent(typeof(XRSimpleInteractable))]
public class SpawnQuiz : MonoBehaviour
{
    [Header("UI & Position")]
    public GameObject infoCanvas; 
    public float heightAboveCube = 0.6f; // Wie hoch über dem Würfel soll es schweben?
    
    private XRSimpleInteractable simpleInteractable;

    void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        
        // Canvas am Start ausblenden und entkoppeln
        if (infoCanvas != null) 
        {
            infoCanvas.SetActive(false);
            // WICHTIG: Canvas aus dem Cube rausholen, damit es nicht mit skaliert
            infoCanvas.transform.SetParent(null); 
        }
    }

    void OnEnable()
    {
        if (simpleInteractable != null)
        {
            // Nur noch auf das Klicken (Select) hören
            simpleInteractable.selectEntered.AddListener(OnSelect); 
        }
    }

    void OnDisable()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnSelect);
        }
    }

    // --- Logik ---

    // Wenn geklickt wird (Trigger)
    private void OnSelect(SelectEnterEventArgs args)
    {
        if (infoCanvas == null) return;

        bool neuerStatus = !infoCanvas.activeSelf;
        
        if (neuerStatus == true) // Wenn wir es gerade ÖFFNEN
        {
            // 1. Position setzen: Genau über dem Würfel
            Vector3 spawnPos = transform.position + (Vector3.up * heightAboveCube);
            infoCanvas.transform.position = spawnPos;

            // 2. Rotation setzen: Canvas soll den Spieler anschauen
            if (Camera.main != null)
            {
                // Richtung zum Kopf des Spielers berechnen
                Vector3 directionToHead = Camera.main.transform.position - spawnPos;
                directionToHead.y = 0; // Wir wollen nicht nach oben/unten kippen
                
                if (directionToHead != Vector3.zero)
                {
                    infoCanvas.transform.rotation = Quaternion.LookRotation(directionToHead);
                }
            }
        }

        // 3. An/Aus schalten
        infoCanvas.SetActive(neuerStatus);
    }
}