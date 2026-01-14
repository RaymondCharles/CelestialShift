using UnityEngine;
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
    }

    private void Refresh()
    {
        if (playerStats == null) return;

        if (healthSlider != null)
            healthSlider.value = playerStats.Health;

        if (hungerSlider != null)
            hungerSlider.value = playerStats.Hunger;
    }
}
