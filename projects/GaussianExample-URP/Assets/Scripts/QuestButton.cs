using UnityEngine;

public class QuestButton : MonoBehaviour
{
    private bool wurdeGeklickt = false;
    private QuestManager meinManager;

    void Start()
    {
        
        meinManager = Object.FindFirstObjectByType<QuestManager>();
    }

    public void ButtonKlickLogik()
    {
    
        if (!wurdeGeklickt)
        {
            wurdeGeklickt = true;
            meinManager.PunktHinzufuegen();
            
        }
    }
}