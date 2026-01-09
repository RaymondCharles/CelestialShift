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
            burnRoutine = StartCoroutine(BurnLoop());
    }

    //private IEnumerator BurnLoop()
    //{
    //    // next tick after 1 second
    //    float nextTick = Time.time + tickInterval;

    //    while (Time.time <= burnUntil + 0.0001f)
    //    {
    //        if (Time.time >= nextTick)
    //        {
    //            if (enemyHealth != null)
    //            {
    //                enemyHealth.TakeDamage(dps); 
    //            }

    //            nextTick += tickInterval;
    //        }

    //        yield return null;
    //    }

    //    burnRoutine = null;
    //    dps = 0f;
    //}

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

        burnRoutine = null;
        dps = 0f;
    }

}
