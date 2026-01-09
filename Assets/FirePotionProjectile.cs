using System.Collections.Generic;
using UnityEngine;

public class FirePotionProjectile : MonoBehaviour
{
    // Splash
    [SerializeField] private float splashRadius = 2f;
    [SerializeField] private LayerMask enemyLayers = ~0;

    // Burn Damage over Time
    [SerializeField] private float damagePerTick = 5f;     // 5 damage
    [SerializeField] private float tickSeconds = 1f;        // every 1 second
    [SerializeField] private float durationSeconds = 5f;    // for 5 seconds

    // Lifetime
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool destroyOnImpact = true;

    private bool exploded;
    private Transform owner;

    public void SetRadius(float value) => splashRadius = value;
    public void SetBurn(float dmgPerTick, float tickSec, float durationSec)
    {
        damagePerTick = dmgPerTick;
        tickSeconds = tickSec;
        durationSeconds = durationSec;
    }

    public void Init(GameObject ownerGO)
    {
        owner = ownerGO != null ? ownerGO.transform : null;
        if (owner == null) return;

        // Ignore collisions with owner
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

        // direct hit first
        TryAffect(directHit, affected);

        // splash
        Collider[] hits = Physics.OverlapSphere(center, splashRadius, enemyLayers, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
            TryAffect(hits[i], affected);

        if (destroyOnImpact)
            Destroy(gameObject);
    }

    private void TryAffect(Collider col, HashSet<GameObject> affected)
    {
        if (col == null) return;

        if (owner != null && col.transform.IsChildOf(owner))
            return;

        EnemyHealth eh = col.GetComponentInParent<EnemyHealth>();
        if (eh == null) return;

        if (!affected.Add(eh.gameObject)) return;

        // Apply burn DoT via EnemyBurn
        var burn = eh.GetComponent<EnemyBurn>();
        if (burn == null) burn = eh.gameObject.AddComponent<EnemyBurn>(); // auto add if missing
        burn.ApplyBurn(damagePerTick, durationSeconds, tickSeconds);

        // Set UI icon timer
        var status = eh.GetComponent<EnemyStatusEffects>();
        if (status == null) status = eh.GetComponentInParent<EnemyStatusEffects>();
        if (status != null)
            status.ApplyBurn(durationSeconds);

        Debug.Log($"FIRE POTION -> {eh.name} burning: {damagePerTick} dmg every {tickSeconds}s for {durationSeconds}s");
    }
}
