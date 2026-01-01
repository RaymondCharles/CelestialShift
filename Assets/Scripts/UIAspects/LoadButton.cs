using UnityEngine;

public class LoadButton : MonoBehaviour
{
    [SerializeField] private int sceneIndexToLoad = 1; // Game scene index

    public void OnClick()
    {
        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.ChangeToGameScene(sceneIndexToLoad);
        }
        else
        {
            Debug.LogError("LoadingManager instance not found!");
        }
    }
}
