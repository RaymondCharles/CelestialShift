using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwordAttackController : MonoBehaviour
{
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private SwordBehaviour sword;

    [SerializeField] private float attackCooldown = 0.45f;
    [SerializeField] private float attackWindow = 0.20f;

    private bool isSwinging;
    private float nextAttackTime;

    private void Awake()
    {
        if (controller == null) controller = GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        if (controller != null && !controller.CanMove) return;

        if (sword == null)
            sword = FindObjectOfType<SwordBehaviour>();

        if (sword == null) return;

        bool pressedAttack = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (!pressedAttack) return;

        if (Time.time < nextAttackTime) return;
        if (isSwinging) return;

        StartCoroutine(SwingRoutine());
    }

    //private IEnumerator SwingRoutine()
    //{
    //    isSwinging = true;
    //    nextAttackTime = Time.time + attackCooldown;

    //    sword.BeginAttack();
    //    yield return new WaitForSeconds(attackWindow);
    //    sword.EndAttack();

    //    isSwinging = false;
    //}

    private IEnumerator SwingRoutine()
    {
        isSwinging = true;
        nextAttackTime = Time.time + attackCooldown;

        Debug.Log("[SwordAttackController] Swing START");
        sword.BeginAttack();

        yield return new WaitForSeconds(attackWindow);

        sword.EndAttack();
        Debug.Log("[SwordAttackController] Swing END");

        isSwinging = false;
    }



}
