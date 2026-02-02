using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameMenu : MonoBehaviour
{
    [Header("Toggle Input (Input System)")]
    public InputActionProperty toggleMenuAction;
    public bool enableInEditorKeyboard = true;

    [Header("Fader")]
    public MenuFader menuFader;
    public ScreenFader titleFader;
    bool busy;

    [Header("Last Scene")]
    public string lastScene = "Titlescreen";

    [Header("UI")]
    public GameObject menuRoot;
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public Transform head;

    [Header("Placement")]
    public float distance = 1.4f;
    public float heightOffset = -0.1f;

    [Header("Pause")]
    public bool pauseTime = true;

    [Header("Anti-Spam")]
    public float toggleCooldown = 0.25f;

    float _nextToggle;
    bool _isOpen;

    void OnEnable() => toggleMenuAction.action?.Enable();
    void OnDisable() => toggleMenuAction.action?.Disable();

    void Start()
    {
        if (!head && Camera.main) head = Camera.main.transform;

        if (optionsPanel) optionsPanel.SetActive(false);
        if (menuFader == null && menuRoot) menuFader = menuRoot.GetComponent<MenuFader>();
        if (mainPanel) mainPanel.SetActive(true);
        if (menuFader != null) menuFader.Hide();
        else if (menuRoot) menuRoot.SetActive(false);

        if (pauseTime) Time.timeScale = 1f;
    }

    void Update()
    {
        bool pressed = false;

        if (toggleMenuAction.action != null && toggleMenuAction.action.WasPressedThisFrame())
            pressed = true;

        if (!pressed && enableInEditorKeyboard)
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.mKey.wasPressedThisFrame))
                pressed = true;
        }

        if (pressed)
        {
            if (Time.unscaledTime < _nextToggle) return;

            _nextToggle = Time.unscaledTime + toggleCooldown;
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (_isOpen) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        _isOpen = true;

        if (head && menuRoot)
        {
            Vector3 forwardFlat = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            if (forwardFlat.sqrMagnitude < 0.001f) forwardFlat = head.forward;

            menuRoot.transform.position = head.position + forwardFlat * distance + Vector3.up * heightOffset;
            menuRoot.transform.rotation = Quaternion.LookRotation(forwardFlat, Vector3.up);
        }

        if(optionsPanel) optionsPanel.SetActive(false);
        if (mainPanel) mainPanel.SetActive(true);
        if (menuFader != null) menuFader.Show();
        else if (menuRoot) menuRoot.SetActive(true);

        if (pauseTime) Time.timeScale = 0f;
    }

    public void CloseMenu()
    {
        _isOpen = false;

        if (mainPanel) mainPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);
        if (menuFader != null) menuFader.Hide();
        else if (menuRoot) menuRoot.SetActive(false);

        if (pauseTime) Time.timeScale = 1f;
    }

    public void OnResumeClicked() => CloseMenu();

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        Debug.Log("Quit (im Build wird die App beendet).");
#else
        Application.Quit();
#endif
    }

    public void Continue() 
    {
        Debug.Log("Continue Application");
        CloseMenu();
    }

    public void Options()
    {
        if (!mainPanel || !optionsPanel) return;
        optionsPanel.SetActive(true);
        mainPanel.SetActive(false);
        Debug.Log("Options opened");
    }

    public void BackFromOptions()
    {
        if(!mainPanel || !optionsPanel) return;
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
        Debug.Log("Back to mainpanel");
    }

    public void BackToTitlescreen()
    {
        Debug.Log("Back to Titlescree");
        if (busy) return;
        StartCoroutine(Titlescreen());
    }

    IEnumerator Titlescreen()
    {
        busy = true;
        if (titleFader != null)
            yield return titleFader.FadeTo(1f);

        SceneManager.LoadScene(lastScene);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            Debug.Log("Quit Application");
        #else
            Application.Quit();
        #endif
    }
}