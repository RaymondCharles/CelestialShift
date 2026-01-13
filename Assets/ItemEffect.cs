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
    public AnimationClip throwItemClip;
    public AnimationClip consumeItemClip;
    public AnimationClip useItemClip;
    public AnimationClip swingItemClip;
    private AnimatorOverrideController overrideController;
    int busyTag = Animator.StringToHash("UsingItem");
    int rightArmLayer;

    void Awake()
    {
        overrideController = new AnimatorOverrideController(playerAnimator.runtimeAnimatorController);
        playerAnimator.runtimeAnimatorController = overrideController;
        inputActions = new PlayerInputActions();
        rightArmLayer = playerAnimator.GetLayerIndex("RightArm Layer");
    }

    // Start is called before the first frame update
    void Start()
    {
        inputActions = fpController.inputActions;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(playerAnimator.GetCurrentAnimatorStateInfo(rightArmLayer).tagHash == busyTag || playerAnimator.GetNextAnimatorStateInfo(rightArmLayer).tagHash == busyTag);
        if (InventoryUI.Instance != null) if (fpController.InventoryPanel.activeSelf) return;
        Debug.Log(equippedItem);
        if (equippedItem == null)
        {
            swingable = false;
            throwable = false;
            consumable = false;
        }
        if (inputActions.Player.Use.IsPressed())
        {
            Debug.Log("Made it here");
            if (swingable)
            {
                overrideController["UseItem"] = swingItemClip;
            }
            else if (throwable)
            {
                overrideController["UseItem"] = throwItemClip;
            }
            else if (consumable)
            {
                overrideController["UseItem"] = consumeItemClip;
            }
            else
            {
                overrideController["UseItem"] = useItemClip;
            }
            playerAnimator.SetTrigger("useItem");
            playerAnimator.SetBool("isBlocking", false);
            //equippedItem.use()
        }
        else if (inputActions.Player.Block.IsPressed() && equippedShield != null && (playerAnimator.GetCurrentAnimatorStateInfo(rightArmLayer).tagHash != busyTag && playerAnimator.GetNextAnimatorStateInfo(rightArmLayer).tagHash != busyTag))
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

    /*public void useItem()
    {
        if (equippedItem != null)
        {
            playerAnimator.SetTrigger("useItem");
            playerAnimator.SetBool("isBlocking", false);
            //equippedItem.use() will have its own animator for collider for sword in the function.
        }
    }*/

    public void useBlock()
    {
        if (equippedShield != null)
        {
            playerAnimator.SetBool("isBlocking", true);
            //Set shield animator to true so that the collider is in place.
        }
    }
}
