using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterDungeon : MonoBehaviour
{
    public string targetSceneName;        // Scene to load when button is pressed
    public string targetSpawnID;          // Spawn point ID in that scene
    public GameObject UIPanel;            // The panel to show when player enters trigger
    public Vector3 spawnPos = new Vector3(-50, 20, -30);
    [Header("Optional: Return Settings")]
    public bool isReturnPoint = false;    // Mark if this trigger is for returning


    public void Travel()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("Target scene not set!");
            return;
        }

        // Close the UI panel immediately
        if (UIPanel != null)
            UIPanel.SetActive(false);
            
        FirstPersonController.Instance.SavePlayer();
        // Store data in GameManager
        if (GameManager.Instance != null)
        {
            if (FirstPersonController.Instance == null) Debug.Log("FPC NULL");
            Vector3 playerPos = FirstPersonController.Instance.transform.position;
            GameManager.Instance.checkpointPos = playerPos;
            playerPos = spawnPos;
            GameManager.Instance.TravelToScene(targetSceneName, targetSpawnID);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }

        // Unlock cursor if this is a return point or UI should be shown
        if (isReturnPoint)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
