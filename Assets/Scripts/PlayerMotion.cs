using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerMotion : MonoBehaviour
{
    public static PlayerMotion Instance;

    PlayerInput playerInput;
    InputAction moveAction;
    InputAction InverntoryAction;
    InputAction PauseAction;
    InputAction DropAction;
    public GameObject InventoryPanel;
    public GameObject PausePanel;
    public Transform playerTransform;
    public float speed = 5.0f;
    public int level = 0; // save feature testing

    public void SavePlayer()
    {
        SaveSystem.SavePlayer(this);
        SceneManager.LoadScene("MenuScene");

    }
    //public void LoadPlayer()
    //{
    //    PlayerData data = SaveSystem.LoadPlayer();
    //    level= data.level;
    //    Vector3 position;
    //    position.x = data.position[0];
    //    position.y = data.position[1];
    //    position.z = data.position[2];
    //    transform.position = position;
    //}
    public void LoadPlayer()
    {
        PlayerData data = SaveSystem.LoadPlayer();
        if (data == null)
        {
            Debug.LogError("No saved player data found!");
            return;
        }

        level = data.level;
        Vector3 position = new Vector3(data.position[0], data.position[1], data.position[2]);
        Debug.Log("Loading position: " + position);
        transform.position = position;
    }




    private void Awake()
    {
        // Set up singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        InverntoryAction = playerInput.actions.FindAction("Inventory");
        PauseAction = playerInput.actions.FindAction("Pause");
        DropAction = playerInput.actions.FindAction("Drop");

        if (GameManager.Instance != null)
        {
            Debug.Log("GameManager instance found. loadGame = " + GameManager.Instance.loadGame);

            if (GameManager.Instance.loadGame)
            {
                Debug.Log("Loading player...");
                LoadPlayer();
                GameManager.Instance.loadGame = false;
            }
        }
        else
        {
            Debug.LogError("No GameManager instance found!");
        }

    }

    // Update is called once per frame
    void Update()
    {
        movePlayer();
        if (InverntoryAction.triggered)
        {
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

    }
    public void InventoryPanelShow()
    {
        InventoryPanel.SetActive(!InventoryPanel.activeSelf);
    }
    public void PausePanelShow()
    {
        PausePanel.SetActive(!PausePanel.activeSelf);
    }
    public void PausePanelHideOnClick()
    {
        PausePanel.SetActive(false);
    }
    void movePlayer()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        transform.position += new Vector3(direction.x, 0, direction.y) * Time.deltaTime * speed;
    }
    public void OnClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveAndQuit();
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
    }
    //public void OnClickPlay()
    //{
    //    LoadingManager.Instance.ChangeToGameScene(0);
    //}
    //public void DropSelectedItem()
    //{
    //    if (Inventory.Instance.items.Count > 0)
    //    {
    //        Item selectedItem = Inventory.Instance.items[0];
    //        Vector3 dropPoint = playerTransform.position + playerTransform.forward;
    //        dropPoint.y = playerTransform.position.y;
    //        Inventory.Instance.DropItem(selectedItem, dropPoint);
    //    }
    //    if (HotBarManager.Instance != null)
    //    {
    //        HotBarManager.Instance.RemoveSpriteFromSlot(0);
    //    }
    //}
    public void DropSelectedItem()
    {
        if (HotBarManager.Instance == null) return;

        int selectedSlot = HotBarManager.Instance.selectedSlot;
        Item item = HotBarManager.Instance.slotItems[selectedSlot];
        if (item == null) return;

        if (Inventory.Instance == null) return;
        if (item.worldPrefab == null) return;

       
        Vector3 dropPos = playerTransform.position + playerTransform.forward * 1.5f + Vector3.up * 0.5f;
        Inventory.Instance.GenerateItem(item, dropPos);
        HotBarManager.Instance.ClearSlot(item);
    }





}
