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

    //Functional Options
    [SerializeField] private bool canSprint = true; //check if you can sprint
    [SerializeField] private bool canJump = true; //check if you can jump

    //Controls
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift; //assign left shift key to sprint
    [SerializeField] private KeyCode jumpKey = KeyCode.Space; //assign jump key to jump

    //Movement Parameters
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
   
    //Look Parameters
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f; //upper and lower bound for look speed in the x axis
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f; //upper and lower bound for look speed in the y axis
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f; //how many degrees we can actually look up before the camera will stop moving
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f; //how many degrees we can actually look down before the camera will stop moving

    //Jumping Parameters
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;


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
             

            ApplyFinalMovements();
        }
    }

    private void HandleMovementInput()
    {
        currentInput = new Vector2((IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal")); //check if sprinting otherwise use walk speed instead

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

    private void ApplyFinalMovements()
    {
        if(!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

    }

}
