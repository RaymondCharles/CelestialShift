using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public event Action OnStatsChanged;

    // Values

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

    private void Start()
    {
        // clamp + update UI once on start
        health = Mathf.Clamp(health, 0, maxHealth);
        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        OnStatsChanged?.Invoke();
    }

    private void Update()
    {
        DrainHungerOverTime();
    }

    private void DrainHungerOverTime()
    {
        if (hungerDrainPerSecond <= 0f) return;

        // Convert per-second drain into integer ticks smoothly
        hungerDrainAccumulator += hungerDrainPerSecond * Time.deltaTime;

        if (hungerDrainAccumulator >= 1f)
        {
            int drainAmount = Mathf.FloorToInt(hungerDrainAccumulator);
            hungerDrainAccumulator -= drainAmount;

            SetHunger(hunger - drainAmount);
        }

        // starving damage if hunger empty
        if (hunger <= 0 && starvingDamagePerSecond > 0f)
        {
            // damage as ints over time 
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
        //Debug.Log($"Player took {amount} damage. Health = {health}");
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
