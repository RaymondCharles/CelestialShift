using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Actions/Throw Splash Potion")]
public class ThrowSplashPotionAction : ItemAction
{
    // Projectile
    public GameObject projectilePrefab;
    public float throwSpeed = 18f;

    // Damage
    public float damage = 20f;
    public float splashRadius = 1.75f;

    // Spawn
    public Vector3 spawnOffset = new Vector3(0f, 0.1f, 0.6f);

    public override void Execute(Item item, GameObject gameManager)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("ThrowSplashPotionAction: No projectilePrefab assigned.");
            return;
        }

        // Try to get player
        Transform playerT = null;
        var gm = gameManager.GetComponent<GameManagerTemp>();
        if (gm != null && gm.player != null) playerT = gm.player.transform;

        // Get a camera transform without depending purely on Camera.main
        Transform camT = null;

        // 1) Camera.main if available
        if (Camera.main != null) camT = Camera.main.transform;

        // 2) Fallback: any enabled camera in scene
        if (camT == null)
        {
            var anyCam = Object.FindFirstObjectByType<Camera>();
            if (anyCam != null) camT = anyCam.transform;
        }

        // If still no camera, fallback to player forward
        Vector3 forward = (camT != null) ? camT.forward : (playerT != null ? playerT.forward : Vector3.forward);

        Vector3 spawnPos;
        if (camT != null)
            spawnPos = camT.position + camT.TransformDirection(spawnOffset);
        else if (playerT != null)
            spawnPos = playerT.position + playerT.TransformDirection(spawnOffset);
        else
            spawnPos = Vector3.zero;

        GameObject proj = Object.Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(forward));

        // Configure splash script
        var splash = proj.GetComponent<SplashPotionProjectile>();
        if (splash != null)
        {
            splash.SetDamage(damage);
            splash.SetRadius(splashRadius);
        }
        else
        {
            Debug.LogWarning("ThrowSplashPotionAction: Projectile prefab has no SplashPotionProjectile attached.");
        }

        // Physics throw
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;               // IMPORTANT: projectile should NOT be kinematic
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.velocity = forward.normalized * throwSpeed;
        }
        else
        {
            Debug.LogWarning("ThrowSplashPotionAction: Projectile prefab has no Rigidbody.");
        }

        // Prevent immediate self-hit (ignore collisions with player)
        if (playerT != null)
        {
            Collider[] playerCols = playerT.GetComponentsInChildren<Collider>();
            Collider[] projCols = proj.GetComponentsInChildren<Collider>();

            for (int i = 0; i < playerCols.Length; i++)
                for (int j = 0; j < projCols.Length; j++)
                    Physics.IgnoreCollision(projCols[j], playerCols[i], true);
        }

        Debug.Log($"Threw splash potion: {item.itemName}");
    }
}
