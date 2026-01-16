using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    [Header("Cursor Settings")]
    public bool forceCursorVisible = true; // Set to true to always show cursor

    void Update()
    {
        if (!forceCursorVisible) return;

        // Only enable cursor in SnowBiome scene
        if (SceneManager.GetActiveScene().name == "SnowBiomeDungeon")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
