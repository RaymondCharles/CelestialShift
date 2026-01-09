using UnityEngine;

public class EnemyStatusEffects : MonoBehaviour
{
    // Timers (seconds remaining)
    [SerializeField] private float freezeRemaining;
    [SerializeField] private float poisonRemaining;
    [SerializeField] private float burnRemaining;

    public float FreezeRemaining => freezeRemaining;
    public float PoisonRemaining => poisonRemaining;

    public float BurnRemaining => burnRemaining;

    public bool IsFrozen => freezeRemaining > 0f;
    public bool IsPoisoned => poisonRemaining > 0f;

    public bool IsBurning => burnRemaining > 0f;

    private void Update()
    {
        if (freezeRemaining > 0f) freezeRemaining -= Time.deltaTime;
        if (poisonRemaining > 0f) poisonRemaining -= Time.deltaTime;
        if (burnRemaining > 0f) burnRemaining -= Time.deltaTime;

        if (freezeRemaining < 0f) freezeRemaining = 0f;
        if (poisonRemaining < 0f) poisonRemaining = 0f;
        if (burnRemaining < 0f) burnRemaining = 0f;
    }

    // Refreshes / extends the effect (keeps the longer duration)
    public void ApplyFreeze(float seconds)
    {
        freezeRemaining = Mathf.Max(freezeRemaining, seconds);
    }

    public void ApplyPoison(float seconds)
    {
        poisonRemaining = Mathf.Max(poisonRemaining, seconds);
    }

    public void ApplyBurn(float seconds)
    {
        burnRemaining = Mathf.Max(burnRemaining, seconds);
    }
}
