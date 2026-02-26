using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{
    [Header("Ziel-Einstellungen")]
    [Tooltip("Der exakte Name der Szene, die geladen werden soll.")]
    public string targetSceneName;

    // Diese Methode wird vom XR Simple Interactable aufgerufen
    public void Interact()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log("Szenenwechsel gestartet: Lade " + targetSceneName);
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("Fehler: Kein Ziel-Szenenname im SceneTeleporter auf " + gameObject.name + " angegeben!");
        }
    }
}