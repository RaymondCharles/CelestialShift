using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public event Action OnStatsChanged;

    // Health + Hunger 
  
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxHunger = 100;

    [SerializeField] private int health = 100;
    [SerializeField] private int hunger = 100;

    [SerializeField] private float hungerDrainPerSecond = 1f;
    [SerializeField] private float starvingDamagePerSecond = 2f;

    private float hungerDrainAccumulator;

    public int MaxHealth => maxHealth;
    public int MaxHunger => maxHunger;
    public int Health => health;
    public int Hunger => hunger;


    // XP + Level 

    [SerializeField] private int level = 1;

    [SerializeField] private int currentXP = 0;

    // Linear XP requirement:
    // XPToNext = baseXP + (level-1)*xpPerLevel
    [SerializeField] private int baseXPToNext = 100;
    [SerializeField] private int xpPerLevel = 50;

    public int Level => level;
    public int CurrentXP => currentXP;
    public int XPToNextLevel => Mathf.Max(1, baseXPToNext + (level - 1) * xpPerLevel);

    private void Start()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        hunger = Mathf.Clamp(hunger, 0, maxHunger);

        // clamp XP sanity
        currentXP = Mathf.Max(0, currentXP);
        level = Mathf.Max(1, level);

        OnStatsChanged?.Invoke();
    }

    private void Update()
    {
        DrainHungerOverTime();
    }


    // XP methods

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        // Level up as many times as needed
        while (currentXP >= XPToNextLevel)
        {
            currentXP -= XPToNextLevel;
            LevelUp();
        }

        OnStatsChanged?.Invoke();
    }

    private void LevelUp()
    {
        level++;
        Debug.Log($"PLAYER LEVEL UP! Now Level {level}");


        // Heal(10);
        // Eat(10);
    }

    // Hunger drain (existing)

    private void DrainHungerOverTime()
    {
        if (hungerDrainPerSecond <= 0f) return;

        hungerDrainAccumulator += hungerDrainPerSecond * Time.deltaTime;

        if (hungerDrainAccumulator >= 1f)
        {
            int drainAmount = Mathf.FloorToInt(hungerDrainAccumulator);
            hungerDrainAccumulator -= drainAmount;

            SetHunger(hunger - drainAmount);
        }

        if (hunger <= 0 && starvingDamagePerSecond > 0f)
        {
            float dmg = starvingDamagePerSecond * Time.deltaTime;
            if (dmg > 0f)
                TakeDamage(Mathf.CeilToInt(dmg));
        }
    }

    public void Eat(int amount)
    {
        if (amount <= 0) return;
        SetHunger(hunger + amount);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        SetHealth(health + amount);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        SetHealth(health - amount);
        Debug.Log($"Player took {amount} damage. Health = {health}");
    }

    private void SetHealth(int newValue)
    {
        int clamped = Mathf.Clamp(newValue, 0, maxHealth);
        if (clamped == health) return;

        health = clamped;
        OnStatsChanged?.Invoke();
    }

    private void SetHunger(int newValue)
    {
        int clamped = Mathf.Clamp(newValue, 0, maxHunger);
        if (clamped == hunger) return;

        hunger = clamped;
        OnStatsChanged?.Invoke();
    }
}
