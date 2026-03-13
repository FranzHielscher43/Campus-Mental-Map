using UnityEngine;
using UnityEngine.UI;

public class OpalScreenFlow : MonoBehaviour
{
    public enum Page { Login, Angemeldet, Lernen, Kursangebote }

    [Header("UI")]
    [SerializeField] private Image background;

    [Header("Sprites")]
    [SerializeField] private Sprite htwk_anmelden;
    [SerializeField] private Sprite htwk_angemeldet;
    [SerializeField] private Sprite htwk_lernen;
    [SerializeField] private Sprite htwk_kursangebote;

    [Header("Hotspot roots")]
    [SerializeField] private GameObject hotspotsLogin;
    [SerializeField] private GameObject hotspotsMenu;

    [Header("Buttons (drag & drop)")]
    [SerializeField] private Button btnAnmelden;
    [SerializeField] private Button btnStartseite;
    [SerializeField] private Button btnLernen;
    [SerializeField] private Button btnKursangebote;

    private void Awake()
    {
        if (btnAnmelden) btnAnmelden.onClick.RemoveAllListeners();
        if (btnStartseite) btnStartseite.onClick.RemoveAllListeners();
        if (btnLernen) btnLernen.onClick.RemoveAllListeners();
        if (btnKursangebote) btnKursangebote.onClick.RemoveAllListeners();

        if (btnAnmelden) btnAnmelden.onClick.AddListener(OnClickAnmelden);
        if (btnStartseite) btnStartseite.onClick.AddListener(OnClickStartseite);
        if (btnLernen) btnLernen.onClick.AddListener(OnClickLernen);
        if (btnKursangebote) btnKursangebote.onClick.AddListener(OnClickKursangebote);
    }

    private void Start()
    {
        GoTo(Page.Login);
    }

    public void OnClickAnmelden()
    {
        Debug.Log("CLICK: Anmelden");
        GoTo(Page.Angemeldet);
    }

    public void OnClickStartseite()
    {
        Debug.Log("CLICK: Startseite");
        GoTo(Page.Angemeldet);
    }

    public void OnClickLernen()
    {
        Debug.Log("CLICK: Lernen");
        GoTo(Page.Lernen);
    }

    public void OnClickKursangebote()
    {
        Debug.Log("CLICK: Kursangebote");
        GoTo(Page.Kursangebote);
    }

    private void GoTo(Page page)
    {
        switch (page)
        {
            case Page.Login: background.sprite = htwk_anmelden; break;
            case Page.Angemeldet: background.sprite = htwk_angemeldet; break;
            case Page.Lernen: background.sprite = htwk_lernen; break;
            case Page.Kursangebote: background.sprite = htwk_kursangebote; break;
        }

        bool isLogin = page == Page.Login;
        if (hotspotsLogin) hotspotsLogin.SetActive(isLogin);
        if (hotspotsMenu) hotspotsMenu.SetActive(!isLogin);
    }
}