using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool loadGame = false; // flag to know when to load player

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

    // Called when new scene loads
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (loadGame && scene.name == "GameScene")
        {
            if (PlayerMotion.Instance != null)
            {
                PlayerMotion.Instance.LoadPlayer();
                loadGame = false; // reset flag
            }
            else
            {
                Debug.LogError("PlayerMotion instance not found in GameScene!");
            }
        }
    }

    // Start a new game
    public void NewGame()
    {
        loadGame = false;
        SceneManager.LoadScene("GameScene");
    }

    // Load a saved game
    public void LoadGame()
    {
        if (!SaveSystem.SaveExists())
        {
            Debug.LogWarning("No save found!");
            return;
        }

        loadGame = true;
        SceneManager.LoadScene("GameScene");
    }
}
