using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonLocation : MonoBehaviour
{
    [Header("UI & Scene Settings")]
    public GameObject UIPanel;            // The panel to show when player enters trigger
    public GameObject OtherUIPanel1;
    public GameObject OtherUIPanel2;
    public string targetSceneName;        // Scene to load when button is pressed
    public string targetSpawnID;          // Spawn point ID in that scene



    private void Start()
    {
        if (UIPanel != null)
            UIPanel.SetActive(false);
        else
        {
            if (targetSceneName == "GrassBiomeDungeon"){
                UIPanel = FirstPersonController.Instance.DungeonUIPanelGrass;
                OtherUIPanel1 = FirstPersonController.Instance.DungeonUIPanelSand;
                OtherUIPanel2 = FirstPersonController.Instance.DungeonUIPanelSnow;
            }
            else if (targetSceneName == "TheSandBiomeDungeon")
            {
                UIPanel = FirstPersonController.Instance.DungeonUIPanelSand;
                OtherUIPanel1 = FirstPersonController.Instance.DungeonUIPanelGrass;
                OtherUIPanel2 = FirstPersonController.Instance.DungeonUIPanelSnow;
            }
            else
            {
                UIPanel = FirstPersonController.Instance.DungeonUIPanelSnow;
                OtherUIPanel1 = FirstPersonController.Instance.DungeonUIPanelGrass;
                OtherUIPanel2 = FirstPersonController.Instance.DungeonUIPanelSand;
            }

            Debug.Log("ASSIGNED THE PANELS");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        
        if (!other.CompareTag("Player")) return;

        if (UIPanel == null)
        {
            if (targetSceneName == "GrassBiomeDungeon"){
                UIPanel = FirstPersonController.Instance.DungeonUIPanelGrass;
                OtherUIPanel1 = FirstPersonController.Instance.DungeonUIPanelSand;
                OtherUIPanel2 = FirstPersonController.Instance.DungeonUIPanelSnow;
            }
            else if (targetSceneName == "TheSandBiomeDungeon")
            {
                UIPanel = FirstPersonController.Instance.DungeonUIPanelSand;
                OtherUIPanel1 = FirstPersonController.Instance.DungeonUIPanelGrass;
                OtherUIPanel2 = FirstPersonController.Instance.DungeonUIPanelSnow;
            }
            else
            {
                UIPanel = FirstPersonController.Instance.DungeonUIPanelSnow;
                OtherUIPanel1 = FirstPersonController.Instance.DungeonUIPanelGrass;
                OtherUIPanel2 = FirstPersonController.Instance.DungeonUIPanelSand;
            }
            Debug.Log("ASSIGNED THE PANELS");
        }

        if (UIPanel.active == false && (OtherUIPanel1 == null || OtherUIPanel1.active == false) && (OtherUIPanel2 == null || OtherUIPanel2.active == false))
        {
            UIPanel.SetActive(true);
            FirstPersonController.Instance.DungeonUIPanelExit = UIPanel;
            // Store data in GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.nextSceneName = targetSceneName;
                GameManager.Instance.nextSpawnID = targetSpawnID;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (UIPanel != null && UIPanel.active)
            UIPanel.SetActive(false);
            FirstPersonController.Instance.DungeonUIPanelExit = null;


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
