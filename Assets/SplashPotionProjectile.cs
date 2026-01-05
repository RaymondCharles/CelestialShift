using System.Collections.Generic;
using UnityEngine;

public class SplashPotionProjectile : MonoBehaviour
{
    
    // Damage
    [SerializeField] private float damage = 20f;
    [SerializeField] private float splashRadius = 5f;
    [Tooltip("Which layers count as enemies for splash overlap.")]
    [SerializeField] private LayerMask enemyLayers = ~0;

    // Lifetime
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool destroyOnImpact = true;

    private bool exploded;
    private GameObject owner;

    public void SetDamage(float value) => damage = value;
    public void SetRadius(float value) => splashRadius = value;

    public void Init(GameObject ownerGO)
    {
        owner = ownerGO;
        if (owner == null) return;

        // Ignore collisions with the owner so you don't hit yourself
        var myCols = GetComponentsInChildren<Collider>();
        var ownerCols = owner.GetComponentsInChildren<Collider>();

        foreach (var a in myCols)
            foreach (var b in ownerCols)
                if (a != null && b != null)
                    Physics.IgnoreCollision(a, b, true);
    }

    private void Awake()
    {
        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;

        Vector3 pos = (collision.contactCount > 0) ? collision.GetContact(0).point : transform.position;
        Explode(pos, collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;

        // If triggers are used (some enemies / hitboxes), explode here too
        Explode(transform.position, other);
    }

    // Make directHit optional so you can call Explode(center) if you ever want
    private void Explode(Vector3 center, Collider directHit = null)
    {
        exploded = true;

        var damaged = new HashSet<EnemyHealth>();

        // Apply damage to the direct collider (if it's an enemy)
        TryDamageEnemy(directHit, damaged);

        // Apply splash damage (but HashSet prevents double-damage + multi-colliders)
        Collider[] hits = Physics.OverlapSphere(center, splashRadius, enemyLayers, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
            TryDamageEnemy(hits[i], damaged);

        if (destroyOnImpact)
            Destroy(gameObject);
    }

    private void TryDamageEnemy(Collider col, HashSet<EnemyHealth> damaged)
    {
        if (col == null) return;

        // Don't damage the owner
        if (owner != null && col.transform.IsChildOf(owner.transform))
            return;

        EnemyHealth eh = col.GetComponentInParent<EnemyHealth>();
        if (eh == null) return;

        // Prevent double damage (direct + splash) and prevent multi-collider spam
        if (!damaged.Add(eh)) return;

        eh.TakeDamage(damage);
        Debug.Log($"SplashPotion -> {eh.gameObject.name} took {damage}. HP now: {eh.CurrentHealth}");
    }
}
