using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action OnDied;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyDelay = 0f;

    private bool isDead;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        if (amount <= 0f) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            isDead = true;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated.");

        // Stop navigation if present
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        // Disable colliders (optional but helps stop interactions)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        OnDied?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject, destroyDelay);
    }
}
