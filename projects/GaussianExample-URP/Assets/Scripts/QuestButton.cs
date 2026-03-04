using UnityEngine;

public class QuestButton : MonoBehaviour
{
    [SerializeField] private QuestManager meinManager;
    private bool wurdeGeklickt = false;

    void Awake()
    {
        if (!meinManager)
            Debug.LogError("QuestManager im Inspector nicht zugewiesen!");
    }

    public void ButtonKlickLogik()
    {
        Debug.Log("[QuestButton] Klick: " + name);

        if (wurdeGeklickt || !meinManager) return;
        wurdeGeklickt = true;
        meinManager.PunktHinzufuegen();
    }
}