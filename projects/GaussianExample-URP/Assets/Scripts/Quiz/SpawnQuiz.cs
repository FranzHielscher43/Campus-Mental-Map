using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; 

[RequireComponent(typeof(XRSimpleInteractable))]
public class SpawnQuiz : MonoBehaviour
{
    [Header("UI & Position")]
    public GameObject infoCanvas; 
    public float heightAboveCube = 0.6f; 

    [Header("Rewards UI")]
    public GameObject reward_ui_1;
    public GameObject reward_ui_2;
    public GameObject reward_ui_3;
    public GameObject fallbackText;

    [Header("Reward Object")]
    public GameObject reward;

    [Header("Door Teleporter")]
    public GameObject doorTeleporter;

    [Header("Current Scene")]
    public string scene;
    
    private XRSimpleInteractable simpleInteractable;

    void Awake()
    {
        simpleInteractable = GetComponent<XRSimpleInteractable>();
        
        if (infoCanvas != null) 
        {
            infoCanvas.SetActive(false);
            infoCanvas.transform.SetParent(null); 
        }

        reward_ui_1.SetActive(false);
        reward_ui_2.SetActive(false);
        reward_ui_3.SetActive(false);
        fallbackText.SetActive(true);
        doorTeleporter.SetActive(false);
    }

    void OnEnable()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.AddListener(OnSelect); 
        }
    }

    void OnDisable()
    {
        if (simpleInteractable != null)
        {
            simpleInteractable.selectEntered.RemoveListener(OnSelect);
        }
    }

    private void OnSelect(SelectEnterEventArgs args)
    {
        if (infoCanvas == null) return;

        bool neuerStatus = !infoCanvas.activeSelf;
        
        if (neuerStatus == true) 
        {
            Vector3 spawnPos = transform.position + (Vector3.up * heightAboveCube);
            infoCanvas.transform.position = spawnPos;

            if (Camera.main != null)
            {
                Vector3 directionToHead = Camera.main.transform.position - spawnPos;
                directionToHead.y = 0; 
                
                if (directionToHead != Vector3.zero)
                {
                    infoCanvas.transform.rotation = Quaternion.LookRotation(directionToHead) * Quaternion.Euler(0f, 180f, 0f);
                }
            }
        }
        infoCanvas.SetActive(neuerStatus);
    }

    public void UnlockRewardForThisScene()
    {
        Debug.Log("[SpawnQuiz] Reward freischalten!");

        if (scene == "VR_Labor")
        {
            reward_ui_1.SetActive(true);
            reward.SetActive(false);
            fallbackText.SetActive(false);
            doorTeleporter.SetActive(true);
        }
        else if (scene == "Mocap_Labor")
        {
            reward_ui_1.SetActive(true);
            reward_ui_2.SetActive(true);
            reward.SetActive(false);
            fallbackText.SetActive(false);
            doorTeleporter.SetActive(true);            
        }
        else if (scene == "Mensa_Automat")
        {
            reward_ui_1.SetActive(true);
            reward_ui_2.SetActive(true);
            reward_ui_3.SetActive(true);
            reward.SetActive(false);
            fallbackText.SetActive(false);
            doorTeleporter.SetActive(true);
        }
    }
}