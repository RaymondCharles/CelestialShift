using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    // References
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

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

    private Vector3 lastPlayerPosition;
    private Vector3 playerVelocity;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 720f;      // degrees per second, very snappy

    // Detection Ranges
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float engagementRange = 10f;

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    private void Awake()
    {
        if (playerTransform == null)
        {
            // Make sure this matches your player object name in the scene
            GameObject playerObj = GameObject.Find("FirstPersonController");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        if (navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        // We will control rotation manually for fast tracking
        if (navAgent != null)
        {
            navAgent.updateRotation = false;
        }

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
        }
    }

    private void Update()
    {
        // Approximate player velocity each frame (for prediction)
        if (playerTransform != null)
        {
            Vector3 currentPos = playerTransform.position;
            playerVelocity = (currentPos - lastPlayerPosition) / Time.deltaTime;
            lastPlayerPosition = currentPos;
        }

        DetectPlayer();
        UpdateBehaviourState();
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
            // Distance and travel time
            Vector3 toTarget = targetPos - firePoint.position;
            float distance = toTarget.magnitude;

            float travelTime = distance / projectileSpeed;
            travelTime = Mathf.Clamp(travelTime, 0f, maxLeadTime);

            // Predict future position
            aimPos = targetPos + playerVelocity * travelTime;
        }

        Vector3 shootDir = (aimPos - firePoint.position).normalized;

        Rigidbody projectileRb = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDir)
        ).GetComponent<Rigidbody>();

        // Fire straight at predicted position
        projectileRb.velocity = shootDir * projectileSpeed;

        Destroy(projectileRb.gameObject, 3f);
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
    }

    private void PerformPatrol()
    {
        if (!hasPatrolPoint)
        {
            FindPatrolPoint();
        }

        if (hasPatrolPoint)
        {
            navAgent.SetDestination(currentPatrolPoint);
            RotateTowardsMovement();
        }

        if (Vector3.Distance(transform.position, currentPatrolPoint) < 1f)
        {
            hasPatrolPoint = false;
        }
    }

    private void PerformChase()
    {
        if (playerTransform != null)
        {
            navAgent.SetDestination(playerTransform.position);
            RotateTowardsPlayer();
        }
    }

    private void PerformAttack()
    {
        // Stop moving while attacking
        if (navAgent != null)
        {
            navAgent.SetDestination(transform.position);
        }

        RotateTowardsPlayer(); // fast tracking while firing

        if (!isOnAttackCooldown)
        {
            FireProjectile();
            StartCoroutine(AttackCooldownRoutine());
        }
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

    // ----- Rotation helpers -----

    private void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;

        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0f; // keep upright

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // So the AI faces the direction it's moving while patrolling
    private void RotateTowardsMovement()
    {
        if (navAgent == null) return;

        Vector3 velocity = navAgent.desiredVelocity;
        velocity.y = 0f;

        if (velocity.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(velocity);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
