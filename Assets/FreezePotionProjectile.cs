using System.Collections.Generic;
using UnityEngine;

public class FreezePotionProjectile : MonoBehaviour
{
    // Damage
    [SerializeField] private float damage = 5f;              // “-5 health” = deal 5 damage
    [SerializeField] private float splashRadius = 2f;
    [SerializeField] private LayerMask enemyLayers = ~0;

    // Freeze
    [SerializeField] private float freezeSeconds = 7f;       // 5–10 seconds (set in Inspector)

    // Lifetime
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool destroyOnImpact = true;

    private bool exploded;
    private Transform owner; // player transform

    public void SetDamage(float value) => damage = value;
    public void SetRadius(float value) => splashRadius = value;
    public void SetFreezeSeconds(float value) => freezeSeconds = value;

    public void Init(GameObject ownerGO)
    {
        owner = ownerGO != null ? ownerGO.transform : null;

        if (owner == null) return;

        // Ignore collisions with player (prevents instant self-hit)
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

        Vector3 pos = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        Explode(pos, collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (exploded) return;
        Explode(transform.position, other);
    }

    private void Explode(Vector3 center, Collider directHit)
    {
        exploded = true;

        var affected = new HashSet<GameObject>();

        // Direct hit
        TryAffect(directHit, affected);

        // Splash
        Collider[] hits = Physics.OverlapSphere(center, splashRadius, enemyLayers, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
            TryAffect(hits[i], affected);

        if (destroyOnImpact)
            Destroy(gameObject);
    }

    private void TryAffect(Collider col, HashSet<GameObject> affected)
    {
        if (col == null) return;

        // Don’t affect player / owner's children
        if (owner != null && col.transform.IsChildOf(owner))
            return;

        // Find EnemyHealth
        EnemyHealth eh = col.GetComponentInParent<EnemyHealth>();
        if (eh == null) return;

        // Prevent double from multiple colliders
        if (!affected.Add(eh.gameObject)) return;

        // Apply tiny damage
        if (damage > 0f)
            eh.TakeDamage(damage);

        // For UI icon: set "frozen" timer
        var status = eh.GetComponent<EnemyStatusEffects>();
        if (status == null) status = eh.GetComponentInParent<EnemyStatusEffects>();
        if (status != null)
            status.ApplyFreeze(freezeSeconds);

        // For gameplay freeze: keep your existing freeze component
        EnemyFreeze freeze = eh.GetComponent<EnemyFreeze>();
        if (freeze == null) freeze = eh.GetComponentInParent<EnemyFreeze>();
        if (freeze != null)
            freeze.Freeze(freezeSeconds);

        Debug.Log($"FREEZE POTION -> {eh.name} took {damage} and froze for {freezeSeconds}s. HP now: {eh.CurrentHealth}");
    }
}
