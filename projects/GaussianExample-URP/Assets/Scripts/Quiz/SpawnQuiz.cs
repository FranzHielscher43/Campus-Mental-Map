using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnQuiz: MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject infoCanvas; 
    
    [Header("Hover Effekt")]
    public Color highlightColor = Color.cyan;
    private Color originalColor;
    private Material cubeMat;

    void Start()
    {
        if (infoCanvas != null) infoCanvas.SetActive(false);
        cubeMat = GetComponent<Renderer>().material;
        originalColor = cubeMat.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cube leuchtet beim Drüberfahren
        cubeMat.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        cubeMat.color = originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (infoCanvas != null)
        {
            // Panel an/aus schalten
            // Das Follower-Skript kümmert sich um die Positionierung!
            infoCanvas.SetActive(!infoCanvas.activeSelf);
        }
    }
}
