using UnityEngine;

public class SceneSettings : MonoBehaviour
{
    [Tooltip("Wenn true: beim Laden dieser Scene kein Fade-In abspielen.")]
    [Header("Fade Verhalten")]
    public bool disableFadeIn = false;

    [Header("Fade Texte")]
    [TextArea] public string fadeOutText;
    [TextArea] public string fadeInText;
}

