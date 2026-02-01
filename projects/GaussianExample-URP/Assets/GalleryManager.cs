using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour
{
    public GameObject galleryPanel; 
    public Image displayImage;      
    public List<Sprite> images;     
    private int currentIndex = 0;

    public void OpenGallery()
    {
        galleryPanel.SetActive(true);
        UpdateDisplay();
    }

    public void CloseGallery()
    {
        galleryPanel.SetActive(false);
    }

    public void NextImage()
    {
        currentIndex = (currentIndex + 1) % images.Count;
        UpdateDisplay();
    }

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