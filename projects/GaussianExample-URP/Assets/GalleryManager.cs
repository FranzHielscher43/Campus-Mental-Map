using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour
{
    public GameObject galleryPanel; // Das Panel
    public Image displayImage;      // Die Anzeige-Komponente
    public List<Sprite> images;     // Deine Liste an Bildern (Sprites)

    private int currentIndex = 0;

    // Öffnet die Galerie
    public void OpenGallery()
    {
        galleryPanel.SetActive(true);
        UpdateDisplay();
    }

    // Schließt die Galerie
    public void CloseGallery()
    {
        galleryPanel.SetActive(false);
    }

    // Nächstes Bild
    public void NextImage()
    {
        currentIndex = (currentIndex + 1) % images.Count;
        UpdateDisplay();
    }

    // Vorheriges Bild
    public void PreviousImage()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = images.Count - 1;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (images.Count > 0)
        {
            displayImage.sprite = images[currentIndex];
        }
    }
}