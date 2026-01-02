using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool loadGame = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (loadGame && scene.name == "SampleScene")
        {
            StartCoroutine(LoadPlayerWhenReady());
        }
    }

    IEnumerator LoadPlayerWhenReady()
    {
        // Wait for scene load
        yield return null;

        // Wait until inventory exists
        while (Inventory.Instance == null ||
               InventoryUI.Instance == null ||
               ItemDatabase.Instance == null)
        {
            yield return null;
        }

        FirstPersonController.Instance.LoadPlayer();
    }


    public void NewGame()
    {
        loadGame = false;
        SceneManager.LoadScene("SampleScene"); 
    }

    public void LoadGame()
    {
        if (!SaveSystem.SaveExists())
        {
            Debug.LogWarning("No save found!");
            return;
        }

        loadGame = true;
        SceneManager.LoadScene("SampleScene");
    }
}
