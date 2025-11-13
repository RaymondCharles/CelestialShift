using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // NEW INPUT SYSTEM

public class FirstPersonController : MonoBehaviour
{
    // New Input System object
    private PlayerInputActions inputActions;

    //Determine whether a player/character is in control
    public bool CanMove { get; private set; } = true;

    // Input-based booleans now use Input Actions
    private bool IsSprinting => canSprint && inputActions.Player.Sprint.IsPressed();
    private bool ShouldJump => canJump && inputActions.Player.Jump.triggered && characterController.isGrounded;
    private bool ShouldCrouch => canCrouch && inputActions.Player.Crouch.triggered && !duringCrouchAnimation && characterController.isGrounded;
    private bool CanSlide => inputActions.Player.Slide.triggered && characterController.isGrounded;
    private bool ContinueSlide => inputActions.Player.Slide.IsPressed() && characterController.isGrounded;
    private bool isSliding = false; //check if you can slide

    //Functional Options
    [SerializeField] private bool canSprint = true; //check if you can sprint
    [SerializeField] private bool canJump = true; //check if you can jump
    [SerializeField] private bool canCrouch = true; //check if you can jump
    [SerializeField] private bool canUseHeadbob = true; //check if you can head bob
    [SerializeField] private bool WillSlideOnSlopes = true; //check if you can slide

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
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
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

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection; //store current movement direction as a 3d vector

    // New input values
    private Vector2 moveInput;     // WASD from Input System
    private Vector2 lookInput;     // Mouse delta from Input System

    private float rotationX = 0;

    [Header("Sliding Settings")]
    public float slideAcceleration = 10f;   // Downhill acceleration
    public float slideDeceleration = 5f;    // Uphill deceleration
    public float slopeThreshold = 0.1f;     // Minimum slope to count as slide

    public Vector3 slideVelocity;

    //Mouse Settings
    [SerializeField] private float mouseSmoothTime = 0.03f; // 0.03–0.08 is good

    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Instantiate the input actions
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // Read input from the New Input System each frame
        moveInput = inputActions.Player.Move.ReadValue<Vector2>(); // x = horizontal, y = vertical
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canJump)
            {
                HandleJump();
            }

            if (canCrouch)
            {
                HandleCrouch();
            }

            if (canUseHeadbob && !isSliding)
            {
                HandleHeadbob();
            }

            // Start slide on press
            if (CanSlide && !isSliding)
            {
                Debug.Log("Start Slide");
                StartSlide(characterController.velocity);
                isSliding = true;
            }

            // Stop slide when button no longer held
            if (isSliding && !ContinueSlide)
            {
                Debug.Log("Stop Slide");
                isSliding = false;
            }

            // While sliding
            if (isSliding)
            {
                Debug.Log("Is Sliding");
                HandleSlide();
            }

            ApplyFinalMovements();
        }
    }

    private void HandleMovementInput()
    {
        // Determine current speed based on state
        float currentSpeed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;

        // moveInput.y = Vertical (W/S), moveInput.x = Horizontal (A/D)
        float targetX = currentSpeed * moveInput.y;   // forward/back
        float targetZ = currentSpeed * moveInput.x;   // left/right

        // Keep existing y velocity (for jump / gravity)
        float moveDirectionY = moveDirection.y;

        // Convert local input into world space movement
        Vector3 forwardMovement = transform.TransformDirection(Vector3.forward) * targetX;
        Vector3 rightMovement = transform.TransformDirection(Vector3.right) * targetZ;

        moveDirection = forwardMovement + rightMovement;
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook()
    {
        // Target delta is raw input * sensitivity
        Vector2 targetMouseDelta = new Vector2(
            lookInput.x * lookSpeedX,
            lookInput.y * lookSpeedY
        );

        // Smooth from current value to target value
        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            targetMouseDelta,
            ref currentMouseDeltaVelocity,
            mouseSmoothTime
        );

        // Vertical (pitch)
        rotationX -= currentMouseDelta.y;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        // Horizontal (yaw)
        transform.rotation *= Quaternion.Euler(0, currentMouseDelta.x, 0);
    }

    private void HandleJump()
    {
        if (ShouldJump)
        {
            moveDirection.y = jumpForce;
        }
    }

    private void HandleCrouch()
    {
        if (ShouldCrouch)
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

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // If you want slope sliding based on the surface normal, re-enable the code here.

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator CrouchStand()
    {
        //Prevent clipping onto the ceiling when crouching
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
        {
            yield break;
        }

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

    public void StartSlide(Vector3 currentVelocity)
    {
        slideVelocity = currentVelocity;
        Vector3 facingDirection = transform.forward;
        facingDirection.y = 0f;
        facingDirection.Normalize();

        slideVelocity += (10 * facingDirection);
    }

    public void HandleSlide()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            Vector3 slopeNormal = hit.normal;
            Vector3 slopeDirection = Vector3.Cross(Vector3.Cross(Vector3.up, slopeNormal), slopeNormal).normalized;
            float slopeAngle = Vector3.Angle(Vector3.up, slopeNormal);

            // Flatten the orientation forward vector (ignore vertical tilt)
            Vector3 facingDirection = transform.forward;
            facingDirection.y = 0f;
            facingDirection.Normalize();

            // Determine alignment of facing vs slope
            float alignment = Vector3.Dot(facingDirection, slopeDirection);

            // Keep only the part of velocity aligned with player facing
            Vector3 projectedVelocity = Vector3.Project(slideVelocity, facingDirection);
            slideVelocity.x = projectedVelocity.x;
            slideVelocity.z = projectedVelocity.z;

            if (slopeAngle >= slopeThreshold)
            {
                // Accelerate along the slope
                slideVelocity += slopeDirection * slideAcceleration * Time.deltaTime * Mathf.Abs(alignment);
            }

            // Apply gravity
            slideVelocity.y -= gravity * Time.deltaTime;

            moveDirection = slideVelocity;
        }
    }
}
