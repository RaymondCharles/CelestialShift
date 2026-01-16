using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonLocation : MonoBehaviour
{
    [Header("UI & Scene Settings")]
    public GameObject UIPanel;            // The panel to show when player enters trigger
    public string targetSceneName;        // Scene to load when button is pressed
    public string targetSpawnID;          // Spawn point ID in that scene

    [Header("Optional: Return Settings")]
    public bool isReturnPoint = false;    // Mark if this trigger is for returning

    private void Start()
    {
        if (UIPanel != null)
            UIPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (UIPanel != null)
            UIPanel.SetActive(true);



        // Store data in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.nextSceneName = targetSceneName;
            GameManager.Instance.nextSpawnID = targetSpawnID;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (UIPanel != null)
            UIPanel.SetActive(false);


    }

    // Call this function from the UI button
    //public void Travel()
    //{
    //    if (string.IsNullOrEmpty(targetSceneName))
    //    {
    //        Debug.LogWarning("Target scene not set!");
    //        return;
    //    }

    //    if (GameManager.Instance != null)
    //    {
    //        GameManager.Instance.TravelToScene(targetSceneName, targetSpawnID);
    //    }
    //    else
    //    {
    //        // Fallback
    //        SceneManager.LoadScene(targetSceneName);
    //    }
    //}

    public void Travel()
    {
      
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("Target scene not set!");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TravelToScene(targetSceneName, targetSpawnID);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }

        // Unlock cursor if this is a return point or UI should be shown
        if (isReturnPoint || UIPanel != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
