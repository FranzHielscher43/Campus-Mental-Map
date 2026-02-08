using UnityEngine;

public class AnimationToggler : MonoBehaviour
{
    [Header("3D Objekt Einstellungen")]
    public GameObject animationParent; 
    public Animator mainAnimator;      

    private bool isVisible = false;

    void Start()
    {
        if (animationParent != null)
        {
            animationParent.SetActive(false);
        }
    }

    public void ToggleAnimation()
    {
        if (animationParent == null) return;

        isVisible = !isVisible;
        
        animationParent.SetActive(isVisible);

        if (isVisible && mainAnimator != null)
        {
            
            mainAnimator.Play(0, -1, 0f);
            Debug.Log("Animation gestartet");
        }
        else
        {
            Debug.Log("Animation versteckt");
        }
    }
}