using UnityEngine;

public class LoadButton : MonoBehaviour
{
    public void OnClick()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadGame();
        else
            Debug.LogError("GameManager instance not found!");
    }
}
