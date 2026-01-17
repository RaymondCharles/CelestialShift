using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotBarSelect : MonoBehaviour
{
    public Image[] Slots;         
    public PlayerInput playerInput;   

    private InputAction hotbarAction;

    
    void Awake()
    {
        if (FirstPersonController.Instance != null) playerInput = FirstPersonController.Instance.GetComponent<PlayerInput>();
        else Debug.Log("FIRST PERSON CONTROLLER IS NULL");
    }
    void Start()
    {
        if (HotBarManager.Instance == null)
        {
            Debug.LogWarning("HotBarManager instance not found. HotBarSelect will not function.");
            return;
        }
        if (FirstPersonController.Instance != null) playerInput = FirstPersonController.Instance.GetComponent<PlayerInput>();
        if (playerInput == null) return;
        hotbarAction = playerInput.actions["HotBarSelect"];
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
    //private void SelectSlot(int index)
    //{
    //    // Highlight the pressed slot
    //    for (int i = 0; i < Slots.Length; i++)
    //        if (Slots[i] != null) { 
    //    Slots[i].gameObject.SetActive(i== index && !Slots[index].gameObject.activeSelf);
    //}

    //    // Tell HotBarManager which slot is currently selected
    //    if (HotBarManager.Instance != null)
    //        if (!Slots[index].gameObject.activeSelf) HotBarManager.Instance.selectedSlot = -1;
    //        else
    //        {
    //            HotBarManager.Instance.selectedSlot = index;
    //            HotBarManager.Instance.UpdateSelectedItem();
    //        }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= Slots.Length)
            return;

        for (int i = 0; i < Slots.Length; i++)
        {
            if (!Slots[i])
                continue;
            

            Slots[i].gameObject.SetActive(i == index && !Slots[i].gameObject.active);
        }

        // Tell HotBarManager which slot is currently selected
        if (HotBarManager.Instance != null)
            if (!Slots[index].gameObject.activeSelf) HotBarManager.Instance.selectedSlot = -1;
            else
            {
                HotBarManager.Instance.selectedSlot = index;
            }
            HotBarManager.Instance.UpdateSelectedItem();
    }

    //}

}
