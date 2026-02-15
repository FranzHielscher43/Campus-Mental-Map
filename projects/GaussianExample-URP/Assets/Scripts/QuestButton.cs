using UnityEngine;

public class QuestButton : MonoBehaviour
{
    private bool wurdeGeklickt = false;
    private QuestManager meinManager;

    void Start()
    {
        // Sucht automatisch den Manager in der Szene
        meinManager = Object.FindFirstObjectByType<QuestManager>();
    }

    public void ButtonKlickLogik()
    {
        // Nur wenn dieser spezielle Button noch nicht gezählt wurde
        if (!wurdeGeklickt)
        {
            wurdeGeklickt = true;
            meinManager.PunktHinzufuegen();
            
            // Optional: Button zur Bestätigung ausgrauen
            // GetComponent<UnityEngine.UI.Image>().color = Color.gray;
        }
    }
}