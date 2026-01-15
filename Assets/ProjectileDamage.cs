using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] private int damage = 5;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private bool destroyOnHit = true;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // Works if projectile collider is not trigger
    private void OnCollisionEnter(Collision collision)
    {
        TryDamage(collision.collider);
    }

    // Works if projectile collider is trigger
    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider hit)
    {
        if (hit == null) return;

        // Try find PlayerStats on the object or its parents 
        PlayerStats stats = hit.GetComponent<PlayerStats>();
        if (stats == null) stats = hit.GetComponentInParent<PlayerStats>();

        if (stats != null)
        {
            stats.TakeDamage(damage);
            Debug.Log($"Projectile hit {hit.name} -> dealt {damage} damage. Health now: {stats.Health}");
        }
        else
        {
            Debug.Log($"Projectile hit {hit.name} but no PlayerStats found.");
        }

        if (destroyOnHit) Destroy(gameObject);
    }
}
