using UnityEngine;

public class ObjectQuestTrigger : MonoBehaviour
{
    private bool wurdeSchonGezaehlt = false;
    private QuestManager meinManager;

    void Start()
    {
        meinManager = Object.FindFirstObjectByType<QuestManager>();
        
        if (meinManager == null)
        {
            Debug.LogError("Kein QuestManager in der Szene gefunden!");
        }
    }

    private void OnMouseDown()
    {
        VersuchePunktZuGeben();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            VersuchePunktZuGeben();
        }
    }

    void VersuchePunktZuGeben()
    {
        
        if (!wurdeSchonGezaehlt && meinManager != null)
        {
            wurdeSchonGezaehlt = true; 
            
            
            meinManager.PunktHinzufuegen(); 
            
            Debug.Log("3D-Modell gezählt! Gesamtcounter steigt.");
        }
    }
}