using UnityEngine;

public class CanvasPersistent : MonoBehaviour
{
    private static CanvasPersistent instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Kill duplicates
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}