using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Endscreen : MonoBehaviour
{
    [Header("UI")]
    public GameObject endscreenRoot;
    public GameObject endscreenPanel;
    public Transform head;

    [Header("Fade")]
    public ScreenFader fader;
    private bool busy;

    [Header("Next Scene")]
    public string nextScene = "TitleScreen";

    [Header("Placement")]
    public float distance = 1.4f;
    public float heightOffset = -0.1f;

    [Header("Anti-Spam")]
    public float toggleCooldown = 0.25f;

    void Start()
    {
        if (!head && Camera.main)
            head = Camera.main.transform;

        if (endscreenPanel)
            endscreenPanel.SetActive(true);
    }

    public void StartApplication()
    {
        Debug.Log("Start gedrückt");
        FinalStartGame();
    }

    public void FinalStartGame()
    {
        if (busy) return;
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        busy = true;

        if (fader != null)
            yield return fader.FadeTo(1f);

        SceneManager.LoadScene(nextScene);
    }
}