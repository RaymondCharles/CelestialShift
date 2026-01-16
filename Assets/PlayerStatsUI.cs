using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;

    private void Awake()
    {
        // Auto-find if not assigned
        if (playerStats == null)
        {
            GameObject playerObj = GameObject.Find("mainCharacter");
            if (playerObj != null) playerStats = playerObj.GetComponent<PlayerStats>();
        }
    }

    private void OnEnable()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged += Refresh;
    }

    private void OnDisable()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged -= Refresh;
    }

    private void Start()
    {
        if (playerStats == null)
        {
            FirstPersonController player = FirstPersonController.Instance;
            if (player != null)
            {
                playerStats = player.GetComponent<PlayerStats>();
            }
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStatsUI: playerStats not found! Make sure the persistent player has PlayerStats component.");
            enabled = false; // disable script to prevent further errors
            return;
        }
        if (playerStats == null)
        {
            Debug.LogError("PlayerStatsUI: playerStats not assigned and mainCharacter not found.");
            enabled = false;
            return;
        }

        // Init ranges once
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = playerStats.MaxHealth;
        }

        if (hungerSlider != null)
        {
            hungerSlider.minValue = 0;
            hungerSlider.maxValue = playerStats.MaxHunger;
        }

        // Update values once at start
        Refresh();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Refresh()
    {
        if (playerStats == null) return;

        if (healthSlider != null)
            healthSlider.value = playerStats.Health;

        if (hungerSlider != null)
            hungerSlider.value = playerStats.Hunger;

        if (playerStats.Health <= 0)
        {
            TriggerGameOver();

            UpdateCrosshair();
        }
        if (xpSlider != null)
        {
            xpSlider.maxValue = playerStats.XPToNextLevel;
            xpSlider.value = playerStats.CurrentXP;
        }

        if (levelText != null)
            levelText.text = $"Lv. {playerStats.Level}";
    }
    private void UpdateCrosshair()
    {

        bool shouldHide = gameOverPanel.activeSelf;

        if (Crosshair != null)
            Crosshair.SetActive(!shouldHide);
    }
    private void TriggerGameOver()
    {
        if (isGameOver) return; 
        isGameOver = true;

        Time.timeScale = 0f; 


        if (GameManager.Instance != null)
        {
            GameManager.Instance.inGame = false; 
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        UpdateCrosshair();
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f; 
        if (GameManager.Instance != null)
        {
            GameManager.Instance.inGame = false; 
        }
        SceneManager.LoadScene("MenuScene");
    }




}
