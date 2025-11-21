  using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMotion : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction InverntoryAction;
    InputAction PauseAction;
    public GameObject InventoryPanel;
    public GameObject PausePanel;
    public float speed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        InverntoryAction = playerInput.actions.FindAction("Inventory");
        PauseAction = playerInput.actions.FindAction("Pause");
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
    public void OnClickPlay()
    {
        LoadingManager.Instance.ChangeToGameScene(0);
    }

}
