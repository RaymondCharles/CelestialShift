using UnityEngine;

public class LoadButton : MonoBehaviour
{
    public int gameSceneIndex = 5; 


    public void OnClickLoadGame()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        if (!SaveSystem.SaveExists())
        {
            Debug.LogWarning("No save found!");
            return;
        }

        GameManager.Instance.loadGame = true;

        if (LoadingManager.Instance != null)
        {
            Debug.Log(gameSceneIndex);
            Debug.Log("Happening 1");
            LoadingManager.Instance.ChangeToGameScene(gameSceneIndex);
        }
        else
        {
            Debug.Log("Happening 2");
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneIndex);
        }
    }
}
