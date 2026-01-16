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
            LoadingManager.Instance.ChangeToGameScene(gameSceneIndex);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneIndex);
        }
    }
}
