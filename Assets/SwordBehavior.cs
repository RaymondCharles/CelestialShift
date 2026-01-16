using System.Collections.Generic;
using UnityEngine;

public class SwordBehaviour : MonoBehaviour
{
    
    [SerializeField] private float swordDamage = 20f;

    public bool canAttack = true; // true = NOT currently swinging
    private readonly HashSet<EnemyHealth> enemiesHit = new HashSet<EnemyHealth>();

    // Call this when the player starts a swing
    public void BeginAttack()
    {
        canAttack = false;      // during swing -> can deal damage
        enemiesHit.Clear();
    }

    // Call this when the swing ends
    public void EndAttack()
    {
        canAttack = true;       // not swinging -> no damage
        enemiesHit.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        // Only damage while swinging
        if (canAttack) return;

        // Must be tagged Enemy (you said enemies are on Enemy layer too, but we use tag to match your prototype)
        if (!other.CompareTag("Enemy")) return;

        // Find EnemyHealth on the object or parent
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth == null) enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null) return;

        // prevent multi-hit per swing
        if (enemiesHit.Contains(enemyHealth)) return;

        enemiesHit.Add(enemyHealth);
        enemyHealth.TakeDamage(swordDamage);

        Debug.Log($"SWORD HIT -> {enemyHealth.name} took {swordDamage}");
    }
}
