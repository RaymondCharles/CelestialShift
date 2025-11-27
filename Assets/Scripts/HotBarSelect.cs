using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotBarSelect : MonoBehaviour
{
    public Image[] Slots;         
    public PlayerInput playerInput;   

    private InputAction hotbarAction;

    void Start()
    {
        hotbarAction = playerInput.actions["HotbarSelect"];
        hotbarAction.performed += OnHotbarPressed;
        hotbarAction.Enable();
    }

    private void OnHotbarPressed(InputAction.CallbackContext ctx)
    {
        int index = hotbarAction.GetBindingIndexForControl(ctx.control);
        SelectSlot(index);
    }

    //private void SelectSlot(int index)
    //{
    //    for (int i = 0; i < Slots.Length; i++)
    //        Slots[i].gameObject.SetActive(i == index);
    //}
    private void SelectSlot(int index)
    {
        // Highlight the pressed slot
        for (int i = 0; i < Slots.Length; i++)
            if (Slots[i] != null) { 
        Slots[i].gameObject.SetActive(i == index);
    }

        // Tell HotBarManager which slot is currently selected
        if (HotBarManager.Instance != null)
            HotBarManager.Instance.selectedSlot = index;
    }

}
