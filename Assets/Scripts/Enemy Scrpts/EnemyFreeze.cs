using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFreeze : MonoBehaviour
{
    // Freeze Visual (Optional)
    [SerializeField] private GameObject freezeVfx; // optional: assign an ice particle or mesh

    private NavMeshAgent agent;
    private Animator animator;

    // If your enemy AI script is called HostileAI (you have this), we’ll freeze it too.
    private MonoBehaviour hostileAI;

    private Coroutine freezeRoutine;
    private bool isFrozen;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        // If you have HostileAI on the root, this will grab it.
        hostileAI = GetComponent<MonoBehaviour>(); // fallback (not great)
        var ai = GetComponent<HostileAI>();
        if (ai != null) hostileAI = ai;

        if (freezeVfx != null) freezeVfx.SetActive(false);
    }

    public void Freeze(float seconds)
    {
        if (seconds <= 0f) return;

        // Refresh freeze duration if hit again
        if (freezeRoutine != null) StopCoroutine(freezeRoutine);
        freezeRoutine = StartCoroutine(FreezeRoutine(seconds));
    }

    private IEnumerator FreezeRoutine(float seconds)
    {
        isFrozen = true;

        // Stop movement / pathfinding
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // Stop AI behaviour (attacks/chasing)
        if (hostileAI != null) hostileAI.enabled = false;

        // Pause animations
        if (animator != null) animator.speed = 0f;

        // Optional VFX
        if (freezeVfx != null) freezeVfx.SetActive(true);

        yield return new WaitForSeconds(seconds);

        // Restore
        if (freezeVfx != null) freezeVfx.SetActive(false);

        if (animator != null) animator.speed = 1f;

        if (hostileAI != null) hostileAI.enabled = true;

        if (agent != null)
        {
            agent.isStopped = false;
        }

        isFrozen = false;
        freezeRoutine = null;
    }
}
