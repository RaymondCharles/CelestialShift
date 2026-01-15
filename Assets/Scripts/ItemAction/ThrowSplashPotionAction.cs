using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Actions/Throw Splash Potion Action")]
public class ThrowSplashPotionAction : ItemAction
{
    // Projectile
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float throwSpeed = 18f;

    // Damage
    [SerializeField] private float damage = 20f;
    [SerializeField] private float splashRadius = 1.75f;

    // Spawn
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1.4f, 0.6f);
    [SerializeField] private float spawnForwardPush = 0.25f; // helps avoid spawning inside colliders

    public override void Execute(Item item, GameObject gameManager)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("ThrowSplashPotionAction: projectilePrefab is NULL (assign it in the asset).");
            return;
        }

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("ThrowSplashPotionAction: Player (tag=Player) not found.");
            return;
        }

        Camera cam = Camera.main;

        // 1) Find hand fire point
        Transform firePoint = FindFirePoint(player.transform);

        // 2) Spawn pos from hand (fallback to offset)
        Vector3 spawnPos = (firePoint != null)
            ? firePoint.position
            : player.transform.position + player.transform.TransformDirection(spawnOffset);

        // 3) Aim point from camera center (shoot where you look)
        Vector3 aimPoint = spawnPos + player.transform.forward * 10f;
        if (cam != null)
        {
            Ray r = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(r, out RaycastHit hit, 500f, ~0, QueryTriggerInteraction.Ignore))
                aimPoint = hit.point;
            else
                aimPoint = r.origin + r.direction * 50f;
        }

        // 4) Direction from HAND -> aim point
        Vector3 forward = (aimPoint - spawnPos).normalized;
        if (forward.sqrMagnitude < 0.0001f)
            forward = player.transform.forward;

        // push forward so it doesn't spawn inside hand/player collider
        spawnPos += forward * spawnForwardPush;

        // 5) Spawn projectile
        GameObject proj = Object.Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(forward));

        // 6) Configure projectile
        var splash = proj.GetComponent<SplashPotionProjectile>();
        if (splash != null)
        {
            splash.SetDamage(damage);
            splash.SetRadius(splashRadius);
            splash.Init(player); // ignore self-collisions
        }
        else
        {
            Debug.LogWarning("ThrowSplashPotionAction: Projectile prefab has no SplashPotionProjectile attached.");
        }

        // 7) Launch
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.velocity = forward * throwSpeed;
        }

        Debug.Log($"Threw splash potion: {item.itemName} (spawn={(firePoint ? "PotionFirePoint" : "offset")})");
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
