using UnityEngine;

public class EnemyStatusEffects : MonoBehaviour
{
    // Timers 
    [SerializeField] private float freezeRemaining;
    [SerializeField] private float poisonRemaining;

    public float FreezeRemaining => freezeRemaining;
    public float PoisonRemaining => poisonRemaining;

    public bool IsFrozen => freezeRemaining > 0f;
    public bool IsPoisoned => poisonRemaining > 0f;

    private void Update()
    {
        if (freezeRemaining > 0f) freezeRemaining -= Time.deltaTime;
        if (poisonRemaining > 0f) poisonRemaining -= Time.deltaTime;

        if (freezeRemaining < 0f) freezeRemaining = 0f;
        if (poisonRemaining < 0f) poisonRemaining = 0f;
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
}
