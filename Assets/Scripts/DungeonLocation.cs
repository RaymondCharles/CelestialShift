using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonLocation : MonoBehaviour
{
    public GameObject DungeonUIPanel;
    public string dungeonSceneName;   // The scene to load
    public string spawnPointID;       // The spawn point ID in that scene

    private void Start()
    {
        DungeonUIPanel.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (DungeonUIPanel != null)
            DungeonUIPanel.SetActive(true);
        else
            Debug.LogError("DungeonUIPanel is NOT assigned!");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        GameManager.Instance.nextDungeonScene = dungeonSceneName;
        GameManager.Instance.nextSpawnPointID = spawnPointID;
        GameManager.Instance.enteringDungeon = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DungeonUIPanel.SetActive(false);
        }
    }

    // This is called by the UI button
    public void EnterDungeon()
    {
        if (!string.IsNullOrEmpty(dungeonSceneName))
        {
            // Load the dungeon scene
            SceneManager.LoadScene(dungeonSceneName);
        }
        else
        {
            Debug.LogWarning("Dungeon scene name not set!");
        }
    }
}
