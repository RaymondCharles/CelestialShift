using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public bool loadGame;

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

    public void NewGame()
    {
        loadGame = false;
        SceneManager.LoadScene("GameScene");
    }

    //public void LoadGame()
    //{
    //    Debug.Log("Load button pressed");

    //    if (!SaveSystem.SaveExists())
    //    {
    //        Debug.Log("No save found!");
    //        return;
    //    }

    //    loadGame = true;
    //    SceneManager.LoadScene("GameScene");
    //}
    public void LoadGame()
    {
        if (!SaveSystem.SaveExists())
        {
            Debug.Log("No save found!");
            return;
        }

        loadGame = true;

        string sceneName = "GameScene"; // EXACT name from Build Settings
        Debug.Log("Loading scene: " + sceneName);

        // Use async to be sure it’s called
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void SaveAndQuit()
    {
        if (PlayerMotion.Instance != null)
            PlayerMotion.Instance.SavePlayer();

        SceneManager.LoadScene("MenuScene");
    }
}
