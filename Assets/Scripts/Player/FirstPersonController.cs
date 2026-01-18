using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // NEW INPUT SYSTEM
using UnityEngine.SceneManagement;
public class FirstPersonController : MonoBehaviour
{
    // New Input System object
    public PlayerInputActions inputActions;
    PlayerInput playerInput;
    InputAction InventoryAction;
    InputAction PauseAction;
    InputAction DropAction;
    InputAction MapAction;
    InputAction MapZoomAction;
    public GameObject DungeonUIPanelExit;
    public GameObject DungeonUIPanelSnow;
    public GameObject DungeonUIPanelGrass;
    public GameObject DungeonUIPanelSand;
    public GameObject InventoryPanel;
    public GameObject GameOverPanel;
    public GameObject HotBar;
    public GameObject PausePanel;
    public GameObject SettingsPanel;
    public GameObject BigMapPanel;
    public GameObject MiniMapPanel; 
    public GameObject RecipePanel;
    public Transform playerTransform;
    public static FirstPersonController Instance;
    public GameObject gameManager;
    private bool isBigMapOpen = false;
    public GameObject Crosshair;


    //Determine whether a player/character is in control
    public bool CanMove { get; private set; } = true;

    // Input-based booleans now use Input Actions
    private bool IsSprinting => canSprint && inputActions.Player.Sprint.IsPressed();
    private bool ShouldJump => canJump && inputActions.Player.Jump.triggered && characterController.isGrounded;
    private bool ShouldCrouch => canCrouch && inputActions.Player.Crouch.triggered && !duringCrouchAnimation && characterController.isGrounded;
    private bool CanSlide => inputActions.Player.Slide.triggered && characterController.isGrounded;
   
    private bool isSliding = false; //check if you can slide
    public bool swordAttack = false;
    public bool shieldDefend = false;

    //Functional Options
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool WillSlideOnSlopes = true;

    //Movement Parameters
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;

    //Look Parameters
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    //Jumping Parameters
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 30.0f;

    //Crouch Parameters
    [SerializeField] private float crouchHeight;
    [SerializeField] private float standingHeight;
    [SerializeField] private float timeToCrouch;
    [SerializeField] private Vector3 crouchingCenter;
    [SerializeField] private Vector3 standingCenter;
    private bool isCrouching;
    private bool duringCrouchAnimation;

    //Headbob Parameters
    [SerializeField] private float walkBobSpeed;
    [SerializeField] private float walkBobAmount;
    [SerializeField] private float sprintBobSpeed;
    [SerializeField] private float sprintBobAmount;
    [SerializeField] private float crouchBobSpeed;
    [SerializeField] private float crouchBobAmount;
    private float defaultYPos = 0;
    private float timer;


    //Animator
    [SerializeField] private Animator playerAnimator;

    // Sliding Parameters
    private Vector3 hitPointNormal; //Normal Position of the Surface you are walking on

    public Camera playerCamera;
    public Transform cameraPos;
    private CharacterController characterController;

    private Vector3 moveDirection; //store current movement direction as a 3d vector

    // New input values
    private Vector2 moveInput;     // WASD from Input System
    private Vector2 lookInput;     // Mouse delta from Input System

    private float rotationX = 0;

    [Header("Enemy Collision")]
    [SerializeField] private LayerMask enemyLayer;   // set in Inspector to the Enemy layer

    [Header("Sliding Settings")]
    public float slideAcceleration = 10f;   // Downhill acceleration
    public float slideDeceleration = 5f;    // Uphill deceleration
    public float slopeThreshold = 0.1f;     // Minimum slope to count as slide

    public Vector3 slideVelocity;

    //Mouse Settings
    [SerializeField] private float mouseSmoothTime = 0.03f; // 0.03-0.08 is good

    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;


    public CameraController cameraController;
    [SerializeField] private float turnSpeed = 12f; // degrees per second


    public bool isGameOver = false;
    public bool inDungeon = false;





    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;


        // Instantiate the input actions
        inputActions = new PlayerInputActions();
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);


    }

    private void Start()
    {

        playerInput = GetComponent<PlayerInput>();
        InventoryAction = playerInput.actions.FindAction("Inventory");
        PauseAction = playerInput.actions.FindAction("Pause");
        DropAction = playerInput.actions.FindAction("Drop");
        MapAction = playerInput.actions.FindAction("Map");
        MapZoomAction = playerInput.actions.FindAction("MapZoom");
        float zoomValue = MapZoomAction.ReadValue<float>();


        /*
        if (GameManager.Instance.loadGame)
        {
            StartCoroutine(DelayedLoadPlayer(1f));
        }
*/
    }
    private IEnumerator DelayedLoadPlayer(float delay)
    {
        CanMove = false;
        yield return new WaitForSeconds(delay);
        LoadPlayer();
        CanMove = true;
    }


    void LateUpdate()
    {
        // Combine all panels safely
        bool anyPanelOpen =
            ((InventoryPanel != null && InventoryPanel.activeSelf) ||
            (PausePanel != null && PausePanel.activeSelf) ||
            (SettingsPanel != null && SettingsPanel.activeSelf) ||
            (GameOverPanel != null && GameOverPanel.activeSelf) ||
            (DungeonUIPanelSnow != null && DungeonUIPanelSnow.activeSelf) ||
            (DungeonUIPanelGrass != null && DungeonUIPanelGrass.activeSelf) ||
            (DungeonUIPanelSand != null && DungeonUIPanelSand.activeSelf) ||
            (DungeonUIPanelExit != null && DungeonUIPanelExit.activeSelf));

        
        // Check current scene
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool alwaysFreeCursorScene = currentScene == "SnowBiomeDungeon";
        bool alwaysFreeCursorScene2 = currentScene == "GrassBiomeDungeon";
        bool alwaysFreeCursorScene3 = currentScene == "TheSandBiomeDungeon";



        if (anyPanelOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RebindSceneUI()
    {
        if (InventoryPanel == null) Debug.Log("IT WAS NUL 11L");
        if (InventoryUI.Instance == null) Debug.Log("IT WAS NUL 22222L");
        InventoryUI.Instance = InventoryPanel.GetComponentInChildren<InventoryUI>(true);
                if (InventoryUI.Instance == null) Debug.Log("IT WAS NUL3 33333 L");
        //InventoryPanel = GameObject.Find("Inventory"); // better: Find by tag or serialized binder
        //HotBar = GameObject.Find("HotBar");
    }



    public void SavePlayer()
    {
        int index = SceneManager.GetActiveScene().buildIndex;
        if (index > 0 && index < 4) return;
        SaveSystem.SavePlayer(this);
        Debug.Log("Player saved at position: " + playerTransform.position);

        if (HotBar != null) PlayerPrefs.SetFloat("HotbarSize", HotBar.transform.localScale.x);
        if (InventoryPanel != null) PlayerPrefs.SetFloat("InvSize", InventoryPanel.transform.localScale.x);
        if (GameManager.Instance != null && GameManager.Instance.currentMusic != null)
            PlayerPrefs.SetFloat("MusicVol", GameManager.Instance.currentMusic.volume);

        PlayerPrefs.Save();
    }



    public void LoadPlayer()
    {
        if (PausePanel.active == true) PausePanelShow();

        PlayerData data = SaveSystem.LoadPlayer();
        if (data == null) return;

        // Load player position
        transform.position = new Vector3(data.position[0], data.position[1] + 20f, data.position[2]);

        // Clear current inventory
        for (int i = 0; i < Inventory.Instance.inventoryItems.Length; i++)
            Inventory.Instance.inventoryItems[i] = null;

        // Load saved inventory
        foreach (InventorySlotData slotData in data.inventorySlots)
        {
            Item item = ItemDatabase.Instance.GetItemByName(slotData.itemName);
            if (item == null) continue;

            Inventory.Instance.inventoryItems[slotData.slotIndex] = new SlotItem(item, slotData.quantity);
        }

        // Update Inventory UI
        if (InventoryUI.Instance != null)
            for (int i = 0; i < Inventory.Instance.inventoryItems.Length; i++)
                InventoryUI.Instance.UpdateSlot(i);

        //HotBarManager.Instance.hotbarSelect.playerInput = Instance.GetComponent<PlayerInput>();
        // Clear hotbar
        for (int i = 0; i < HotBarManager.Instance.slotItems.Length; i++)
        {
            HotBarManager.Instance.slotItems[i] = null;
            HotBarManager.Instance.UpdateSlot(i);
        }

        // Load hotbar items
        foreach (InventorySlotData slotData in data.HotBarSlots)
        {
            Item item = ItemDatabase.Instance.GetItemByName(slotData.itemName);
            if (item == null) continue;

            HotBarManager.Instance.slotItems[slotData.slotIndex] = new SlotItem(item, slotData.quantity);
            HotBarManager.Instance.UpdateSlot(slotData.slotIndex);
        }

        if (data.armorSlot != null && ItemDatabase.Instance.GetItemByName(data.armorSlot.itemName) != null)
        {
            Item armorItem = ItemDatabase.Instance.GetItemByName(data.armorSlot.itemName);
            SlotItem armorSlot = new SlotItem(armorItem, data.armorSlot.quantity);
            ArmorUI.Instance.SetItem(0, armorSlot);
            ArmorUI.Instance.UpdateSlot(0);
        }

        Debug.Log("Player + Inventory/HotBar loaded");


        DayNightCycle.Instance.dayNumber = data.day;
        DayNightCycle.Instance.timeOfDay = Mathf.Repeat(data.timeOfDay, 1f);
        DayNightCycle.Instance.elapsedTime = Mathf.Clamp(data.elapsedTime, 0f, DayNightCycle.Instance.targetDayLength * 60f);
        DayNightCycle.Instance.clockText = data.timeText;


        // Apply HotBar size
        if (HotBar != null)
            HotBar.transform.localScale = Vector3.one * (PlayerPrefs.GetFloat("HotbARSize", 0.6f));

        // Apply Inventory size
        if (InventoryPanel != null)
            InventoryPanel.transform.localScale = Vector3.one * (PlayerPrefs.GetFloat("InvSize", 1f));

        // Apply music volume
        if (GameManager.Instance != null && GameManager.Instance.currentMusic != null)
            GameManager.Instance.currentMusic.volume = PlayerPrefs.GetFloat("MusicVol", 0.6f);

        if (EndlessTerrain.Instance!= null)
        {
            EndlessTerrain.Instance.viewer = this.transform;
        }

        gameManager = GameObject.FindGameObjectWithTag("InGameManager");
        gameManager.GetComponent<GameManagerTemp>().player = Instance.gameObject;
        if (gameManager == null) Debug.Log("NOOOOOO");

    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
    public void InventoryPanelShow()
    {
        bool newState = !InventoryPanel.activeSelf;

        if (!newState)
        {
            Vector3 dropPos = playerTransform.position +
                playerTransform.forward * 1.5f +
                Vector3.up * 0.5f;

            for (int i=0; i<CraftingUI.Instance.craftingItems.Length; i++)
            {
                if (CraftingUI.Instance.craftingItems[i] != null)
                { 
                    Inventory.Instance.DropItem(CraftingUI.Instance.craftingItems[i], dropPos);
                    CraftingUI.Instance.ClearSlot(i);
                }
            }
        }
        if (newState)
        {
            // Close BigMap if open
            if (isBigMapOpen) CloseBigMap();

            // Close PausePanel if open
            if (PausePanel.activeSelf) PausePanel.SetActive(false);
        }

        InventoryPanel.SetActive(newState);

        // Update all slots
        if (Inventory.Instance != null && InventoryUI.Instance != null)
        {
            for (int i = 0; i < Inventory.Instance.inventoryItems.Length; i++)
            {
                InventoryUI.Instance.UpdateSlot(i);
            }
        }

        UpdateCrosshair();
    }

    //public void EnterDungeon()
    //{
    //    SceneManager.LoadScene(GameManager.Instance.nextDungeonScene);
    //}
    //private void UpdateCrosshair()
    //{

    //    bool shouldHide = PausePanel.activeSelf || InventoryPanel.activeSelf || BigMapPanel.activeSelf || SettingsPanel.activeSelf;

    //    if (Crosshair != null)
    //        Crosshair.SetActive(!shouldHide);
    //}

    private void UpdateCrosshair()
    {
        bool shouldHide =
            (PausePanel != null && PausePanel.activeSelf) ||
            (InventoryPanel != null && InventoryPanel.activeSelf) ||
            (BigMapPanel != null && BigMapPanel.activeSelf) ||
            (SettingsPanel != null && SettingsPanel.activeSelf);

        if (Crosshair != null)
            Crosshair.SetActive(!shouldHide);
    }


    public void PausePanelShow()
    {
        if (PausePanel == null) return;

        bool isActive = !PausePanel.activeSelf;

        if (isActive)
        {
            if (InventoryPanel != null && InventoryPanel.activeSelf)
                InventoryPanel.SetActive(false);

            if (isBigMapOpen)
                CloseBigMap();
        }

        PausePanel.SetActive(isActive);

        if (GameManager.Instance != null)
            GameManager.Instance.inGame = !isActive;

        if (isActive)
        {
            Time.timeScale = 0f;
            CanMove = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            CanMove = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        UpdateCrosshair();
    }



    public void ContinueGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.inGame = true;
        Debug.Log("2");
  
        if (PausePanel != null)
            PausePanel.SetActive(false);

        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);

      
        if (InventoryPanel != null && !isBigMapOpen)
            InventoryPanel.SetActive(false);

       
        Time.timeScale = 1f;
        CanMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void ToggleBigMap()
    {
        if (isBigMapOpen)
            CloseBigMap();
        else
            OpenBigMap();
    }

    public void OpenBigMap()
    {
        isBigMapOpen = true;
        BigMapPanel.SetActive(true);
        MiniMapPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UpdateCrosshair();
    }

    public void CloseBigMap()
    {
        isBigMapOpen = false;
        BigMapPanel.SetActive(false);
        MiniMapPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UpdateCrosshair();
    }



    public void OpenSettingsPanel()
    {
        if (PausePanel != null)
        {
            PausePanel.SetActive(false);
        }

        if (SettingsPanel != null) 
        { 
        SettingsPanel.SetActive(true);
        }
        if (InventoryPanel != null)
        {
            InventoryPanel.SetActive(true);
        }
    }

    public void CloseSettingsPanel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.inGame = true; 
        }
        if (SettingsPanel != null)
            SettingsPanel.SetActive(false);

        
        if (PausePanel != null)
            PausePanel.SetActive(false);

        if (InventoryPanel != null)
        { 
        InventoryPanel.SetActive(false);
        }
        Time.timeScale = 1f;
        CanMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RecipePanelShow()
    {
        RecipePanel.SetActive(!RecipePanel.activeSelf);
    }
    public void PausePanelHideOnClick()
    {
        PausePanel.SetActive(false);
    }

    public void OnClickSaveAndQuit()
    {
        
        if (FirstPersonController.Instance != null)
        {
            FirstPersonController.Instance.SavePlayer();
        }
        else
        {
            Debug.LogWarning("Save skipped: FirstPersonController instance not found in this scene.");
        }
        Time.timeScale = 1f;
        CanMove = true;

        if (GameManager.Instance != null)
            GameManager.Instance.loadGame = true;

        // Go to MenuScene via LoadingManager
        if (LoadingManager.Instance != null)
        {
            int menuSceneIndex = 0; 
            LoadingManager.Instance.ChangeToGameScene(menuSceneIndex);
        }
        else
        {
            SceneManager.LoadScene("MenuScene");
        }
    }


    public void DropSelectedItem()
    {
        int selectedSlot = HotBarManager.Instance.selectedSlot;
        if (selectedSlot == -1) return;
        SlotItem slotItem = HotBarManager.Instance.slotItems[selectedSlot];
        if (slotItem == null) return;

        if (Inventory.Instance == null) return;
        if (slotItem.itemDetails.worldPrefab == null) return;


        Vector3 dropPos = playerTransform.position + playerTransform.forward * 1.5f + Vector3.up * 0.5f;

        // Spawn the world item
        Inventory.Instance.GenerateItem(slotItem.itemDetails, slotItem.quantity, dropPos);

        // Remove from hotbar
        HotBarManager.Instance.ClearSlot(slotItem);
    }


    public void UseSelectedItem()
    {
        int selectedSlot = HotBarManager.Instance.selectedSlot;
        if (selectedSlot == -1) return;
        SlotItem slotItem = HotBarManager.Instance.slotItems[selectedSlot];
        Debug.Log("USED");
        if (slotItem == null) return;

        if (Inventory.Instance == null) return;
        if (slotItem.itemDetails.worldPrefab == null) return;

        if (slotItem.quantity > 0)
        {
            /*
            if (gameManager == null)
            {
                gameManager = GameObject.FindGameObjectWithTag("InGameManager");
                gameManager.GetComponent<GameManagerTemp>().player = Instance.gameObject;
            }*/
            slotItem.itemDetails.Use(gameManager);
            if (slotItem.itemDetails.usable) slotItem.quantity--;
            HotBarManager.Instance.UpdateSlot(selectedSlot);
            if (slotItem.quantity == 0)
            {
                slotItem.itemDetails.UnEquip();
                HotBarManager.Instance.ClearSlot(slotItem);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
     
        // Read input from the New Input System each frame
        moveInput = inputActions.Player.Move.ReadValue<Vector2>(); // x = horizontal, y = vertical
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        bool isIdle = true;
        bool isFalling = false;


        if (!GameManagerTemp.Instance.isGameOver && (GameManager.Instance == null || GameManager.Instance.inGame))
        {
            //Inventory 
            if (InventoryAction.triggered)
            {
                if (isBigMapOpen)
                {
                    CloseBigMap();
                }
                InventoryPanelShow();
            }

            if (PauseAction.triggered)
            {
                PausePanelShow();
            }
            if (DropAction.triggered)
            {
                DropSelectedItem();
            }
            if (MapAction.triggered)
            {
                if (InventoryPanel.activeSelf)
                {
                    InventoryPanel.SetActive(false);
                    MiniMapPanel.SetActive(false);
                    OpenBigMap();
                }
                else
                {
                    ToggleBigMap();
                }
            }
        }
        else
        {
            isGameOver = true;
        }


        if (CanMove)
        {
            HandleMovementInput();
            HandleRotation();
            //HandleMouseLook();
            

            if (moveInput.magnitude > 0 || isSliding || isCrouching || !characterController.isGrounded)
            {
                isIdle = false;
            }
            else
            {
                playerAnimator.SetBool("isWalking", false);
                playerAnimator.SetBool("isRunning", false);
                playerAnimator.SetBool("isCrouching", false);
                isIdle = true;
            }

            if (canJump && !isCrouching)
            {
                HandleJump();
            }

            if (canCrouch && !isSliding)
            {
                HandleCrouch();
                if (isCrouching)
                {
                    playerAnimator.SetBool("isCrouching", true);
                }
                else
                {
                    playerAnimator.SetBool("isCrouching", false);
                }
            }

            /*
            if (canUseHeadbob && !isSliding)
            {
                HandleHeadbob();
            }*/

            // Start slide on press
            if (CanSlide && !isSliding && !isCrouching)
            {
                StartSlide(characterController.velocity);
                isSliding = true;
            }

            bool ContinueSlide = inputActions.Player.Slide.IsPressed() && characterController.isGrounded;
            //Debug.Log(ContinueSlide + "ContinueSlide");
            //Debug.Log(isSliding + "isSliding");
            // Stop slide when button no longer held
            if (isSliding && !ContinueSlide)
            {
                //Debug.Log("Stop Slide");
                isSliding = false;
                isIdle = true;
            }

            // While sliding
            if (isSliding)
            {
                //Debug.Log("Is Sliding");
                HandleSlide();
            }
            
            if (!characterController.isGrounded)
            {
               // Debug.Log("Grounded check");
                isFalling = true;
                isIdle = false;
            }

            playerAnimator.SetBool("isIdle", isIdle);
            playerAnimator.SetBool("isFalling", isFalling);
            playerAnimator.SetBool("isSliding", isSliding);
            ApplyFinalMovements();
            
        }
    }
    
    private void HandleRotation()
    {
        Vector3 cameraYaw = new Vector3(0f, cameraController.playerCam.eulerAngles.y, 0f);
        if (cameraController.cameraLock)
        {
            transform.rotation = Quaternion.Euler(cameraYaw);
        }
        else
        {
            if (moveInput.magnitude > 0.01f)
            {
                float offsetDeg = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
                float targetYaw = cameraYaw.y + offsetDeg;
                float currentYaw = transform.eulerAngles.y;

                float smoothYaw = Mathf.LerpAngle(currentYaw,targetYaw,turnSpeed * Time.deltaTime);

                transform.rotation = Quaternion.Euler(new Vector3(0f, smoothYaw, 0f));
            }
        }
    }

    private void HandleMovementInput()
    {
        // Determine current speed based on state
        float currentSpeed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        if (moveInput.magnitude > 0)
        {
            playerAnimator.SetBool("isWalking", !isCrouching && !IsSprinting);
            playerAnimator.SetBool("isRunning", !isCrouching && IsSprinting);
            playerAnimator.SetBool("isCrouching", isCrouching);
        }
        // moveInput.y = Vertical (W/S), moveInput.x = Horizontal (A/D)
        float targetX = currentSpeed * moveInput.y;   // forward/back
        float targetZ = currentSpeed * moveInput.x;   // left/right

        // Keep existing y velocity (for jump / gravity)
        float moveDirectionY = moveDirection.y;

        Vector3 forwardMovement = transform.TransformDirection(Vector3.forward) * targetX;
        Vector3 rightMovement = transform.TransformDirection(Vector3.right) * targetZ;
        if (!cameraController.cameraLock)
        {
            forwardMovement = cameraController.followTarget.TransformDirection(Vector3.forward) * targetX;
            rightMovement = cameraController.followTarget.transform.TransformDirection(Vector3.right) * targetZ;
        }

        moveDirection = forwardMovement + rightMovement;
        moveDirection.y = moveDirectionY;
    }
/*
    private void HandleMouseLook()
    {
         Horizontal (yaw)
        transform.rotation *= Quaternion.Euler(0, playerCamera.transform.localRotation.x, 0);
    }*/

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

    /*private void HandleHeadbob()
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
    }*/

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

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

    // --------- THIS is the key part that stops you climbing enemies ----------
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // If this collider is not on the enemyLayer, ignore.
        if ((enemyLayer.value & (1 << hit.collider.gameObject.layer)) == 0)
            return;

        // Don't let enemy act as a ramp / ground you can climb.
        // Remove any upward movement that would climb along its surface.
        if (moveDirection.y > 0f)
            moveDirection.y = 0f;

        // Also, only slide horizontally along it.
        Vector3 horizontalMove = new Vector3(moveDirection.x, 0f, moveDirection.z);
        if (horizontalMove.sqrMagnitude < 0.0001f)
            return;

        // Use only the horizontal part of the normal so we slide around, not up.
        Vector3 normal = hit.normal;
        normal.y = 0f;

        Vector3 slide = Vector3.ProjectOnPlane(horizontalMove, normal);

        moveDirection.x = slide.x;
        moveDirection.z = slide.z;
    }
}
