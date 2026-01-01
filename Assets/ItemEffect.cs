using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemEffect : MonoBehaviour
{
    public GameObject equippedShield;
    public GameObject equippedItem; //For now it's sword.
    private PlayerInputActions inputActions;
    public FirstPersonController fpController;
    [SerializeField] private Animator playerAnimator;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    // Start is called before the first frame update
    void Start()
    {
        inputActions = fpController.inputActions;
    }

    // Update is called once per frame
    void Update()
    {
        if (InventoryUI.Instance != null) if (fpController.InventoryPanel.activeSelf) return;
        if (inputActions.Player.Use.IsPressed() && equippedItem != null)
        {
            playerAnimator.SetTrigger("useItem");
            playerAnimator.SetBool("shieldDefend", false);
            //equippedItem.use()
        }
        else if (inputActions.Player.Block.IsPressed() && equippedShield != null)
        {
            playerAnimator.SetBool("shieldDefend", true);
        }
        if (inputActions.Player.Use.WasReleasedThisFrame())
        {
            playerAnimator.ResetTrigger("useItem");
        }
        if (inputActions.Player.Block.WasReleasedThisFrame())
        {
            playerAnimator.SetBool("shieldDefend", false);
        }
        
        
    }

    public void useItem()
    {
        if (equippedItem != null)
        {
            Debug.Log("IsHappening");
            playerAnimator.SetTrigger("useItem");
            playerAnimator.SetBool("shieldDefend", false);
            //equippedItem.use() will have its own animator for collider for sword in the function.
        }
    }

    public void useBlock()
    {
        if (equippedShield != null)
        {
            Debug.Log("IsHappening 2");
            playerAnimator.SetBool("shieldDefend", true);
            //Set shield animator to true so that the collider is in place.
        }
    }
}
