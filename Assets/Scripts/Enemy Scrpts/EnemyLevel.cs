using UnityEngine;

public class EnemyLevel : MonoBehaviour
{
    // Level
    [Min(1)] public int level = 1;

    // Base Stats (L1)
    public float baseAttackCooldown = 1f;  // HostileAI attackCooldown at level 1
    public int baseMeleeDamage = 10;       // HostileAI meleeDamage at level 1
    public float baseMeleeHitDelay = 0.2f; // HostileAI meleeHitDelay at level 1

    // Linear scaling per level

    // Cool down reduction per level
    public float cooldownReductionPerLevel = 0.05f;

    // Extra melee damage per level
    public int meleeDamagePerLevel = 2;

    // Hit delay reduction per level
    public float hitDelayReductionPerLevel = 0.01f;

    // Clamps
    public float minAttackCooldown = 0.2f;
    public float minMeleeHitDelay = 0.05f;

    public float ScaledAttackCooldown =>
        Mathf.Max(minAttackCooldown, baseAttackCooldown - (level - 1) * cooldownReductionPerLevel);

    public int ScaledMeleeDamage =>
        Mathf.Max(1, baseMeleeDamage + (level - 1) * meleeDamagePerLevel);

    public float ScaledMeleeHitDelay =>
        Mathf.Max(minMeleeHitDelay, baseMeleeHitDelay - (level - 1) * hitDelayReductionPerLevel);
}
