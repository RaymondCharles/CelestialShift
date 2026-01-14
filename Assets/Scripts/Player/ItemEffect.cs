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
    SwingableCollision itemCollisionScript;
    int busyTag = Animator.StringToHash("UsingItem");
    int rightArmLayer;
    public float useSpeed = 1f;
    public bool midSwing;
    float startTime;
    float len;
    float cooldownUntil = 0f;

    bool isFading = false;

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
        if (InventoryUI.Instance != null) if (fpController.InventoryPanel.activeSelf) return;
        if (equippedItem == null)
        {
            swingable = false;
            throwable = false;
            consumable = false;
        }

        if (inputActions.Player.Use.WasPressedThisFrame() && !midSwing && Time.time >= cooldownUntil)
        {
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
            playerAnimator.SetFloat("useSpeed", useSpeed);
            
            playerAnimator.SetTrigger("useItem");
            playerAnimator.SetBool("isBlocking", false);

            len = overrideController["UseItem"].length / (playerAnimator.speed * useSpeed);
            startTime = Time.time;
            fpController.UseSelectedItem();
        }
        else if (inputActions.Player.Block.IsPressed() && equippedShield != null && !midSwing)
        {
            playerAnimator.SetBool("isBlocking", true);
        }

        if (equippedItem != null && (itemCollisionScript = equippedItem.GetComponent<SwingableCollision>()) != null)
        {
            if (itemCollisionScript.hasHit && !isFading)
            {
                isFading = true;

                float elapsed = Time.time - startTime;
                float remaining = Mathf.Max(0f, len - elapsed);

                Debug.Log("Starting transition" + remaining);
                playerAnimator.CrossFade("Idle2", 0.1f, rightArmLayer);

                cooldownUntil = Mathf.Max(cooldownUntil, Time.time + remaining);

                Invoke("ResetHit", (remaining));
            }
        }


        if (inputActions.Player.Use.WasReleasedThisFrame())
        {
            playerAnimator.ResetTrigger("useItem");
        }
        if (inputActions.Player.Block.WasReleasedThisFrame())
        {
            playerAnimator.SetBool("isBlocking", false);
        }
        
        if (playerAnimator.GetCurrentAnimatorStateInfo(rightArmLayer).tagHash == busyTag || playerAnimator.GetNextAnimatorStateInfo(rightArmLayer).tagHash == busyTag)
        {
            midSwing = true;
        }
        else
        {
            midSwing = false;
        }
        if (equippedItem != null && (itemCollisionScript = equippedItem.GetComponent<SwingableCollision>()) != null) itemCollisionScript.midSwing = midSwing;
    }


    void ResetHit()
    {
        isFading = false;
        if ((itemCollisionScript = equippedItem.GetComponent<SwingableCollision>()) != null)
        {
            itemCollisionScript.ResetHit();
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
