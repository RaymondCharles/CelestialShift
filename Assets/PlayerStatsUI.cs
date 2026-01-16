using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
   
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;

    [SerializeField] private Slider xpSlider;
    [SerializeField] private TMP_Text levelText; // optional

    private void Awake()
    {
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

        if (xpSlider != null)
        {
            xpSlider.minValue = 0;
            xpSlider.maxValue = playerStats.XPToNextLevel;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (playerStats == null) return;

        if (healthSlider != null)
            healthSlider.value = playerStats.Health;

        if (hungerSlider != null)
            hungerSlider.value = playerStats.Hunger;

        if (xpSlider != null)
        {
            xpSlider.maxValue = playerStats.XPToNextLevel;
            xpSlider.value = playerStats.CurrentXP;
        }

        if (levelText != null)
            levelText.text = $"Lv. {playerStats.Level}";
    }
}
