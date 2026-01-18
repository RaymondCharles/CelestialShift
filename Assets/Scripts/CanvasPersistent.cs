using UnityEngine;

public class CanvasPersistent : MonoBehaviour
{
    private static CanvasPersistent instance;
    public GameObject ArmorUIPanel;
    public GameObject HotbarPanel;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("DOING THIS");
            //ArmorUI.Instance = instance.ArmorUIPanel.GetComponent<ArmorUI>();
            Destroy(gameObject); // Kill duplicates
            return;
        }

        instance = this;
        //ArmorUI.Instance = ArmorUIPanel.GetComponent<ArmorUI>();
        DontDestroyOnLoad(gameObject);
    }
}