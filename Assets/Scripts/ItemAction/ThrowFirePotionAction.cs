using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Actions/Throw Fire Potion Action")]
public class ThrowFirePotionAction : ItemAction
{
    // Projectile
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float throwSpeed = 18f;

    // Fire DoT 
    [SerializeField] private float splashRadius = 2f;
    [SerializeField] private float damagePerTick = 5f;
    [SerializeField] private float tickSeconds = 1f;
    [SerializeField] private float durationSeconds = 5f;

    // Spawn
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1.4f, 0.6f);
    [SerializeField] private float spawnForwardPush = 0.25f;

    public override void Execute(Item item, GameObject gameManager)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("ThrowFirePotionAction: Player not found.");
            return;
        }

        Camera cam = Camera.main;
        Transform firePoint = FindFirePoint(player.transform);

        Vector3 spawnPos = firePoint != null
            ? firePoint.position
            : player.transform.position + player.transform.TransformDirection(spawnOffset);

        // Aim from camera center
        Vector3 aimPoint = spawnPos + player.transform.forward * 10f;
        if (cam != null)
        {
            Ray r = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(r, out RaycastHit hit, 500f, ~0, QueryTriggerInteraction.Ignore))
                aimPoint = hit.point;
            else
                aimPoint = r.origin + r.direction * 50f;
        }

        Vector3 forward = (aimPoint - spawnPos).normalized;
        if (forward.sqrMagnitude < 0.0001f) forward = player.transform.forward;

        spawnPos += forward * spawnForwardPush;

        GameObject proj = Object.Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(forward));

        var fire = proj.GetComponent<FirePotionProjectile>();
        if (fire != null)
        {
            fire.SetRadius(splashRadius);
            fire.SetBurn(damagePerTick, tickSeconds, durationSeconds);
            fire.Init(player);
        }
        else
        {
            Debug.LogWarning("ThrowFirePotionAction: Projectile prefab has no FirePotionProjectile attached.");
        }

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.velocity = forward * throwSpeed;
        }

        Debug.Log($"Threw FIRE potion: {item.itemName}");
    }

    private Transform FindFirePoint(Transform playerRoot)
    {
        var all = playerRoot.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
            if (t.name == "PotionFirePoint")
                return t;
        return null;
    }
}
