using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyBurn : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;

    private Coroutine burnRoutine;
    private float burnUntil;
    private float dps;             // damage per second
    private float tickInterval = 1f;

    // VFX
    [SerializeField] private GameObject fireVfxPrefab;     // assign prefab here
    [SerializeField] private Vector3 vfxLocalOffset = new Vector3(0f, 1f, 0f);
    private GameObject fireVfxInstance;


    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();
    }

    // Call this when hit by fire potion
    public void ApplyBurn(float damagePerSecond, float durationSeconds, float tickEverySeconds = 1f)
    {
        
        dps = Mathf.Max(dps, damagePerSecond); 
        tickInterval = Mathf.Max(0.05f, tickEverySeconds);
        burnUntil = Mathf.Max(burnUntil, Time.time + durationSeconds);

        if (burnRoutine == null)
            EnsureFireVfx();
            burnRoutine = StartCoroutine(BurnLoop());
    }

    private IEnumerator BurnLoop()
    {
        while (Time.time < burnUntil)
        {
            // Tick NOW
            if (enemyHealth != null)
                enemyHealth.TakeDamage(dps);

            // Wait for next tick
            yield return new WaitForSeconds(tickInterval);
        }

        StopFireVfx();

        burnRoutine = null;
        dps = 0f;

    }

    private void EnsureFireVfx()
    {
        if (fireVfxPrefab == null) return;
        if (fireVfxInstance != null) return;

        fireVfxInstance = Instantiate(fireVfxPrefab, transform);
        fireVfxInstance.transform.localPosition = vfxLocalOffset;
        fireVfxInstance.transform.localRotation = Quaternion.identity;
    }

    private void StopFireVfx()
    {
        if (fireVfxInstance == null) return;
        Destroy(fireVfxInstance);
        fireVfxInstance = null;
    }

}

