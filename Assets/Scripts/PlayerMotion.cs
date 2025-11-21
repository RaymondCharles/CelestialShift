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
    public GameObject InventoryPanel;
    public float speed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        InverntoryAction = playerInput.actions.FindAction("Inventory");
    }

    // Update is called once per frame
    void Update()
    {
        movePlayer();
        if (InverntoryAction.triggered)
        {
            InventoryPanelShow();
        }
      
    }
    public void InventoryPanelShow()
    {
        InventoryPanel.SetActive(!InventoryPanel.activeSelf);
    }
    void movePlayer()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        transform.position += new Vector3(direction.x, 0, direction.y) * Time.deltaTime * speed;
    }
    
}
