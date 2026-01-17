using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonLocation : MonoBehaviour
{
    [Header("UI & Scene Settings")]
    public GameObject UIPanel;            // The panel to show when player enters trigger
    public string targetSceneName;        // Scene to load when button is pressed
    public string targetSpawnID;          // Spawn point ID in that scene



    private void Start()
    {
        if (UIPanel != null)
            UIPanel.SetActive(false);
        else
        {
            if (targetSceneName == "GrassBiomeDungeon") UIPanel = FirstPersonController.Instance.DungeonUIPanelGrass;
            else if (targetSceneName == "TheSandBiomeDungeon") UIPanel = FirstPersonController.Instance.DungeonUIPanelSand;
            else UIPanel = FirstPersonController.Instance.DungeonUIPanelSnow;
            Debug.Log("ASSIGNED THE PANELS");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (!other.CompareTag("Player")) return;

        if (UIPanel == null)
        {
            if (targetSceneName == "GrassBiomeDungeon") UIPanel = FirstPersonController.Instance.DungeonUIPanelGrass;
            else if (targetSceneName == "TheSandBiomeDungeon") UIPanel = FirstPersonController.Instance.DungeonUIPanelSand;
            else UIPanel = FirstPersonController.Instance.DungeonUIPanelSnow;
            Debug.Log("ASSIGNED THE PANELS");
        }

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

}
