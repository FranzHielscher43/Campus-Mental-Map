using UnityEngine;
using Unity.XR.CoreUtils;

public class HeightManagement : MonoBehaviour
{
    [Header("XR Origin")]
    public XROrigin xrOrigin;

    [Header("Height Settings")]
    public float minHeight = 1.5f;
    public float maxHeight = 2.0f;
    public float defaultHeight = 1.75f;

    void Start() 
    {
        if (!xrOrigin)
            xrOrigin = FindObjectOfType<XROrigin>();

        SetHeight(defaultHeight);
    }

    public void SetHeight(float height)
    {
        height = Mathf.Clamp(height, minHeight, maxHeight);

        Transform cameraOffset = xrOrigin.CameraFloorOffsetObject.transform;

        Vector3 pos = cameraOffset.localPosition;
        pos.y = height;
        cameraOffset.localPosition = pos;

        PlayerPrefs.SetFloat("player_height", height);
        PlayerPrefs.Save();
    }
}
