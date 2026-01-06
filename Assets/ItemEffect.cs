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
    public bool throwable;
    public bool consumable;
    public bool swingable;
    public AnimationClip throwItem;
    public AnimationClip consumeItem;
    public AnimationClip swingItem;
    private AnimatorOverrideController overrideController;

    void Awake()
    {
        overrideController = new AnimatorOverrideController(playerAnimator.runtimeAnimatorController);
        playerAnimator.runtimeAnimatorController = overrideController;
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
        Debug.Log(equippedItem);
        if (inputActions.Player.Use.IsPressed() && equippedItem != null)
        {
            Debug.Log("Made it here");
            if (swingable)
            {
                overrideController["UseItem"] = swingItem;
            }
            else if (throwable)
            {
                overrideController["UseItem"] = throwItem;
            }
            else if (consumable)
            {
                overrideController["UseItem"] = consumeItem;
            }
            else
            {
                overrideController["UseItem"] = null;
            }
            playerAnimator.SetTrigger("useItem");
            playerAnimator.SetBool("isBlocking", false);
            //equippedItem.use()
        }
        else if (inputActions.Player.Block.IsPressed() && equippedShield != null)
        {
            playerAnimator.SetBool("isBlocking", true);
        }
        if (inputActions.Player.Use.WasReleasedThisFrame())
        {
            playerAnimator.ResetTrigger("useItem");
        }
        if (inputActions.Player.Block.WasReleasedThisFrame())
        {
            playerAnimator.SetBool("isBlocking", false);
        }
        
        
    }

    public void useItem()
    {
        if (equippedItem != null)
        {
            Debug.Log("IsHappening");
            playerAnimator.SetTrigger("useItem");
            playerAnimator.SetBool("isBlocking", false);
            //equippedItem.use() will have its own animator for collider for sword in the function.
        }
    }

    public void useBlock()
    {
        if (equippedShield != null)
        {
            Debug.Log("IsHappening 2");
            playerAnimator.SetBool("isBlocking", true);
            //Set shield animator to true so that the collider is in place.
        }
    }
}
