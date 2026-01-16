using System.Collections.Generic;
using UnityEngine;

public class SwordBehaviour : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float swordDamage = 10f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool damageEnabled;
    private readonly HashSet<EnemyHealth> hitThisSwing = new HashSet<EnemyHealth>();

    public void BeginAttack()
    {
        damageEnabled = true;
        hitThisSwing.Clear();
        if (debugLogs) Debug.Log("[Sword] BeginAttack");
    }

    public void EndAttack()
    {
        damageEnabled = false;
        hitThisSwing.Clear();
        if (debugLogs) Debug.Log("[Sword] EndAttack");
    }

    private void OnTriggerEnter(Collider other) => TryHit(other);
    private void OnTriggerStay(Collider other) => TryHit(other);

    private void TryHit(Collider other)
    {
        if (!damageEnabled) return;
        if (other == null) return;

        // Look for EnemyHealth anywhere up the hierarchy
        EnemyHealth enemyHealth =
            other.GetComponent<EnemyHealth>() ??
            other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
        {
            if (debugLogs)
                Debug.Log($"[Sword] Hit {other.name} but NO EnemyHealth");
            return;
        }

        // Prevent multiple hits per swing
        if (!hitThisSwing.Add(enemyHealth)) return;

        enemyHealth.TakeDamage(swordDamage);

        if (debugLogs)
            Debug.Log($"[Sword] HIT {enemyHealth.name} for {swordDamage}");
    }
}
