using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    //Determine whether a player/character is in control
    public bool CanMove { get; private set; } = true;
    //Determine whether or not you can sprint/if you should sprint
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    //Determine whether or not you can jump/if you should jump
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    //Determine whether or not you can crouch/if you should crouch
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;   

    //Functional Options
    [SerializeField] private bool canSprint = true; //check if you can sprint
    [SerializeField] private bool canJump = true; //check if you can jump
    [SerializeField] private bool canCrouch = true; //check if you can jump
    [SerializeField] private bool canUseHeadbob = true; //check if you can head bob
    [SerializeField] private bool WillSlideOnSlopes = true; //check if you can slide

    //Controls
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift; //assign left shift key to sprint
    [SerializeField] private KeyCode jumpKey = KeyCode.Space; //assign jump key to jump
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl; //assign letf control key to crouch


    //Movement Parameters
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;

    //Look Parameters
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f; //upper and lower bound for look speed in the x axis
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f; //upper and lower bound for look speed in the y axis
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f; //how many degrees we can actually look up before the camera will stop moving
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f; //how many degrees we can actually look down before the camera will stop moving

    //Jumping Parameters
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    //Crouch Parameters
    //-> Crouch Height
    [SerializeField] private float crouchHeight = 0.5f;
    //-> Stand Height
    [SerializeField] private float standingHeight = 2f;
    //-> Time to crouch/stand
    [SerializeField] private float timeToCrouch = 0.25f;
    //-> Crouching Center Point
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    //-> Standing Center Point
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    //-> Is Crouching?
    private bool isCrouching;
    //-> Is in crouch animation/mid crouch?
    private bool duringCrouchAnimation;

    //Headbob Parameters
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;

    // Sliding Parameters
    private Vector3 hitPointNormal; //Normal Position of the Surface you are walking on
    private bool IsSliding
    {
        get
        {
            //Debug.DrawRay(transform.position, Vector3.down, Color.red); --> Use this for debugging
            if(characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal; //angle value of floor
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else 
            {
                return false;
            }
        }
    }

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection; //store current movement direction as a 3d vector
    private Vector2 currentInput; //store current keyboard input (W,A,S,D...)

    private float rotationX = 0;


    
    // Start is called before the first frame update
    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>(); 
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;  
        Cursor.visible = false; 
    }

    // Update is called once per frame
    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();
            
            if(canJump)
            {
                HandleJump();
            }

            if(canCrouch)
            {
                HandleCrouch();
            }

            if(canUseHeadbob)
            {
                HandleHeadbob();
            }

            ApplyFinalMovements();
        }
    }

    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed :  walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal")); //check if sprinting otherwise use walk speed instead

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleJump()
    {
        if(ShouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    private void HandleCrouch()
    {
        if(ShouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded)
        {
            return;
        }

        //if grounded apply the headbob effect
        if(Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            //check if walking, sprinting, jumping to apply correct effect
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed: walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);

        }
    }

    private void ApplyFinalMovements()
    {
        if(!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if(WillSlideOnSlopes && IsSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }

        characterController.Move(moveDirection * Time.deltaTime);

    }

    private IEnumerator CrouchStand()
    {
        //Prevent clipping onto the ceiling when crouching
        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            yield break;
        }

        //Lerp from one value to another over a set amount of time ie.: standing height and crouching height
        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;  
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }
}
