using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TeleportGSTestScene();
        }
    }

    public void TeleportGSTestScene()
    {
        SceneManager.LoadScene("GSTestScene");
    }
}