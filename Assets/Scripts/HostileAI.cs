using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    // References
    [SerializeField] private NavMeshAgent navAgent;
    public Transform playerTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] public GameObject projectilePrefab;
    [SerializeField] private EnemyLevel enemyLevel;

    // Layers
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private LayerMask playerLayerMask;

    // Patrol Settings
    [SerializeField] private float patrolRadius = 10f;
    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    // Combat Settings
    [SerializeField] private float attackCooldown = 1f;
    private bool isOnAttackCooldown;

    [Header("Projectile / Aiming Settings")]
    [SerializeField] private float projectileSpeed = 20f;     // how fast the bullet travels
    [SerializeField] private float aimHeightOffset = 1.2f;    // aim a bit above player feet (towards chest/head)
    [SerializeField] private bool usePrediction = true;
    [SerializeField] private float maxLeadTime = 1f;          // clamp prediction time

    public Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 720f;      // degrees per second, very snappy

    // Detection Ranges
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float engagementRange = 10f;

    // Attack Selection (Anti Flip-Flop)
    [Header("Attack Selection (Anti Flip-Flop)")]
    [SerializeField] private float meleeEnterRange = 2.2f;   // start melee when <= this
    [SerializeField] private float meleeExitRange = 3.0f;    // return to ranged only when >= this
    private bool preferMelee;

    // Animation
    [SerializeField] private Animator animator;

    // Parameter names
    [SerializeField] private string speedA = "Speed";
    [SerializeField] private string meleeTrigger = "Melee";
    [SerializeField] private string rangedTrigger = "Ranged";

    // Movement thresholds (tune per-enemy)
    [SerializeField] private float walkSpeedThreshold = 0.1f;
    [SerializeField] private float runSpeedThreshold = 3.0f;

    // What type of attacks this enemy uses
    public enum AttackMode { MeleeOnly, RangedOnly, Both }
    [SerializeField] private AttackMode attackMode = AttackMode.Both;

    // Melee Damage Settings
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private float meleeAngle = 90f;        // total cone angle
    [SerializeField] private float meleeHitDelay = 0.2f;    // hit during the swing

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    // Attack lock (prevents melee/ranged flip-flop)
    private enum AttackChoice { None, Melee, Ranged }
    private AttackChoice lockedChoice = AttackChoice.None;
    private bool isAttacking;

    private void Awake()
    {
        if (playerTransform == null)
        {
            if (FirstPersonController.Instance != null) playerTransform = FirstPersonController.Instance.transform;
        }
        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (enemyLevel == null)
            enemyLevel = GetComponent<EnemyLevel>();

        ApplyLevelStats();

        // We will control rotation manually for fast tracking
        if (navAgent != null)
            navAgent.updateRotation = false;
    }
    

    private void Update()
    {
        // Approximate player velocity each frame (for prediction)
        if (playerTransform != null)
        {
            Vector3 currentPos = playerTransform.position;
            playerVelocity = (currentPos - lastPlayerPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastPlayerPosition = currentPos;
        }

        DetectPlayer();
        UpdateBehaviourState();
        UpdateMovementAnimation();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engagementRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    private void DetectPlayer()
    {
        isPlayerVisible = Physics.CheckSphere(transform.position, visionRange, playerLayerMask);
        isPlayerInRange = Physics.CheckSphere(transform.position, engagementRange, playerLayerMask);
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || playerTransform == null)
            return;

        // Base target: player's current position + height offset
        Vector3 targetPos = playerTransform.position + Vector3.up * aimHeightOffset;
        Vector3 aimPos = targetPos;

        if (usePrediction)
        {
            Vector3 toTarget = targetPos - firePoint.position;
            float distance = toTarget.magnitude;

            float travelTime = distance / Mathf.Max(projectileSpeed, 0.001f);
            travelTime = Mathf.Clamp(travelTime, 0f, maxLeadTime);

            aimPos = targetPos + playerVelocity * travelTime;
        }

        Vector3 shootDir = (aimPos - firePoint.position).normalized;
        if (shootDir.sqrMagnitude < 0.0001f)
            shootDir = transform.forward;

        Quaternion newRotation = Quaternion.LookRotation(shootDir) * Quaternion.Euler(-90f, 180f, 0f);

        // Instantiate GO first (so we can ignore collisions using colliders)
        GameObject projGO = Instantiate(projectilePrefab, firePoint.position, newRotation);

        // IMPORTANT: ignore collisions between THIS ENEMY and the projectile
        IgnoreCollisionsWithSelf(projGO);

        Rigidbody projectileRb = projGO.GetComponent<Rigidbody>();
        if (projectileRb != null)
            projectileRb.velocity = shootDir * projectileSpeed;

        Destroy(projGO, 3f);
    }

    private void IgnoreCollisionsWithSelf(GameObject projectile)
    {
        if (projectile == null) return;

        // Collect colliders on enemy (shooter) + projectile
        var enemyCols = GetComponentsInChildren<Collider>(true);
        var projCols = projectile.GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < projCols.Length; i++)
        {
            var p = projCols[i];
            if (p == null) continue;

            for (int j = 0; j < enemyCols.Length; j++)
            {
                var e = enemyCols[j];
                if (e == null) continue;

                Physics.IgnoreCollision(p, e, true);
            }
        }
    }


    private void FindPatrolPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);

        Vector3 potentialPoint = new Vector3(
            transform.position.x + randomX,
            transform.position.y,
            transform.position.z + randomZ
        );

        if (Physics.Raycast(potentialPoint, -transform.up, 2f, terrainLayer))
        {
            currentPatrolPoint = potentialPoint;
            hasPatrolPoint = true;
        }
    }

    private IEnumerator AttackCooldownRoutine()
    {
        isOnAttackCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnAttackCooldown = false;

        // unlock after cooldown
        isAttacking = false;
        lockedChoice = AttackChoice.None;
    }

    private void PerformPatrol()
    {
        if (!hasPatrolPoint)
            FindPatrolPoint();

        if (hasPatrolPoint)
        {
            navAgent.SetDestination(currentPatrolPoint);
            RotateTowardsMovement();
        }

        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1f)
            hasPatrolPoint = false;
    }

    private void PerformChase()
    {
        if (playerTransform != null)
        {
            navAgent.SetDestination(playerTransform.position);
            RotateTowardsPlayer();
        }
    }

    private AttackChoice ChooseAttack(float dist)
    {
        // Hysteresis to avoid flip-flop at the boundary
        if (!preferMelee)
        {
            if (dist <= meleeEnterRange) preferMelee = true;
        }
        else
        {
            if (dist >= meleeExitRange) preferMelee = false;
        }

        return preferMelee ? AttackChoice.Melee : AttackChoice.Ranged;
    }

    private void PerformAttack()
    {
        if (navAgent != null)
            navAgent.SetDestination(transform.position);

        RotateTowardsPlayer();

        if (playerTransform == null) return;

        // prevent spamming triggers each frame
        if (isAttacking) return;
        if (isOnAttackCooldown) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        AttackChoice choice;
        switch (attackMode)
        {
            case AttackMode.MeleeOnly:
                choice = AttackChoice.Melee;
                break;
            case AttackMode.RangedOnly:
                choice = AttackChoice.Ranged;
                break;
            case AttackMode.Both:
            default:
                choice = ChooseAttack(dist);
                break;
        }

        lockedChoice = choice;
        isAttacking = true;

        if (lockedChoice == AttackChoice.Melee)
        {
            TriggerMelee();
            StartCoroutine(MeleeHitRoutine());
        }
        else
        {
            TriggerRanged();
            StartCoroutine(RangedFireRoutine()); // small delay helps close-range reliability
        }

        StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator RangedFireRoutine()
    {
        // Tiny delay so the ranged trigger "wins" and the enemy doesn't instantly flip to melee visuals
        yield return new WaitForSeconds(0.12f);
        FireProjectile();
    }

    private void UpdateBehaviourState()
    {
        if (!isPlayerVisible && !isPlayerInRange)
        {
            PerformPatrol();
        }
        else if (isPlayerVisible && !isPlayerInRange)
        {
            PerformChase();
        }
        else if (isPlayerVisible && isPlayerInRange)
        {
            PerformAttack();
        }
    }

    // Rotation helpers

    private void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;

        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void RotateTowardsMovement()
    {
        if (navAgent == null) return;

        Vector3 velocity = navAgent.desiredVelocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(velocity);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null || navAgent == null) return;

        float speed = navAgent.velocity.magnitude;
        animator.SetFloat(speedA, speed);
    }

    // Anim triggers

    private void TriggerMelee()
    {
        if (animator == null) return;
        animator.ResetTrigger(rangedTrigger);
        animator.SetTrigger(meleeTrigger);
    }

    private void TriggerRanged()
    {
        if (animator == null) return;
        animator.ResetTrigger(meleeTrigger);
        animator.SetTrigger(rangedTrigger);
    }

    // Melee logic

    private void DoMeleeDamage()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist > meleeEnterRange) return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        float angle = Vector3.Angle(forward, toPlayer);
        if (angle > meleeAngle * 0.5f) return;

        PlayerStats stats = playerTransform.GetComponent<PlayerStats>();
        if (stats == null) stats = playerTransform.GetComponentInParent<PlayerStats>();

        if (stats != null)
        {
            stats.TakeDamage(meleeDamage);
            Debug.Log($"MELEE HIT -> dealt {meleeDamage} damage. Player health now: {stats.Health}");
        }
    }

    private IEnumerator MeleeHitRoutine()
    {
        if (meleeHitDelay > 0f)
            yield return new WaitForSeconds(meleeHitDelay);

        DoMeleeDamage();
    }

    private void ApplyLevelStats()
    {
        if (enemyLevel == null) return;

        attackCooldown = enemyLevel.ScaledAttackCooldown;
        meleeDamage = enemyLevel.ScaledMeleeDamage;
        meleeHitDelay = enemyLevel.ScaledMeleeHitDelay;
    }

}
