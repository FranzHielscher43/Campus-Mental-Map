using UnityEngine;
using UnityEngine.UI;

public class OpalScreenFlow : MonoBehaviour
{
    public enum Page
    {
        Login,
        Angemeldet,      // Startseite
        Lernen,
        Kursangebote
    }

    [Header("UI")]
    [SerializeField] private Image background;

    [Header("Sprites")]
    [SerializeField] private Sprite htwk_anmelden;
    [SerializeField] private Sprite htwk_angemeldet;
    [SerializeField] private Sprite htwk_lernen;
    [SerializeField] private Sprite htwk_kursangebote;

    [Header("Hotspot roots")]
    [SerializeField] private GameObject hotspotsLogin; // Hotspots_Login
    [SerializeField] private GameObject hotspotsMenu;  // Hotspots_Menu

    private Page currentPage;

    private void Start()
    {
        GoTo(Page.Login);
    }

    public void OnClickAnmelden()
    {
        GoTo(Page.Angemeldet);
    }

    public void OnClickStartseite()
    {
        GoTo(Page.Angemeldet);
    }

    public void OnClickLernen()
    {
        GoTo(Page.Lernen);
    }

    public void OnClickKursangebote()
    {
        GoTo(Page.Kursangebote);
    }

    public void GoTo(Page page)
    {
        currentPage = page;
        switch (page)
        {
            case Page.Login:
                background.sprite = htwk_anmelden;
                break;
            case Page.Angemeldet:
                background.sprite = htwk_angemeldet;
                break;
            case Page.Lernen:
                background.sprite = htwk_lernen;
                break;
            case Page.Kursangebote:
                background.sprite = htwk_kursangebote;
                break;
        }
        
        bool isLogin = page == Page.Login;
        if (hotspotsLogin) hotspotsLogin.SetActive(isLogin);
        if (hotspotsMenu) hotspotsMenu.SetActive(!isLogin);
    }
}