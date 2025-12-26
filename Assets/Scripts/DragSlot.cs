using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
public class DragSlot : MonoBehaviour, IBeginDragHandler, IDragHandler ,IEndDragHandler
{
    public int index;
    public SlotType slotType;
    public Canvas canvas;

    public CanvasGroup group;
    public Image dragIcon;

    private Item draggedItem;

    public void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventdata)
    {
        //Item item = HotBarManager.Instance.slotItems[index];
        //if (item == null)
        //{
        //    return;
        //}
        draggedItem = GetItem();
        if (draggedItem == null)
        {
            return;
        }

        dragIcon = new GameObject("dragIcon").AddComponent<Image>();
        dragIcon.transform.SetParent(canvas.transform, false);
        dragIcon.transform.SetAsLastSibling();
        dragIcon.sprite = draggedItem.itemImg;
        //dragIcon.color = new Color(1, 1, 1, 0.8f);
        dragIcon.rectTransform.sizeDelta = new Vector2(50, 50);
        dragIcon.raycastTarget = false;

        group.alpha = 0.5f;

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }

    }

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    group.alpha = 1f;
    //    if (dragIcon != null)
    //    {
    //        Destroy(dragIcon);
    //    }
    //    if (eventData.pointerEnter == null)
    //    {
    //        return;
    //    }
    //    DragSlot targetslot = eventData.pointerEnter.GetComponentInParent<DragSlot>();
    //    if (targetslot != null)
    //    {
    //        HandleSlotSwap(targetslot);
    //    }

    //}
    public void OnEndDrag(PointerEventData eventData)
    {
        group.alpha = 1f;

        if (dragIcon != null)
            Destroy(dragIcon);

        DragSlot targetSlot = eventData.pointerEnter?.GetComponentInParent<DragSlot>();
        if (targetSlot != null)
        {
            HandleSlotSwap(targetSlot);
            return;
        }

      
        if (draggedItem != null && Inventory.Instance != null && PlayerMotion.Instance != null)
        {
            Vector3 dropPos = PlayerMotion.Instance.playerTransform.position + PlayerMotion.Instance.playerTransform.forward;
            dropPos.y = PlayerMotion.Instance.playerTransform.position.y;

            Inventory.Instance.GenerateItem(draggedItem, dropPos);

            if (slotType == SlotType.Hotbar && HotBarManager.Instance != null)
            {
                HotBarManager.Instance.ClearSlot(draggedItem);
            }
            else if (slotType == SlotType.Inventory && InventoryUI.Instance != null)
            {
                InventoryUI.Instance.inventoryItems[index] = null;
                InventoryUI.Instance.UpdateSlot(index);
            }
        }

        draggedItem = null;
    }


    private Item GetItem()
    {
        if (slotType == SlotType.Hotbar)
            return HotBarManager.Instance.slotItems[index];
        else if (slotType == SlotType.Inventory)
            return InventoryUI.Instance.inventoryItems[index];
        return null;
    }


    void HandleSlotSwap(DragSlot target)
    {
        if (target == null) return;  
        if ((slotType == SlotType.Inventory || target.slotType == SlotType.Inventory)
            && InventoryUI.Instance == null) return; 

        if (slotType == SlotType.Hotbar && target.slotType == SlotType.Hotbar)
        {
            HotBarManager.Instance.SlotSwap(index, target.index);
            return;
        }

        if (slotType == SlotType.Inventory && target.slotType == SlotType.Inventory)
        {
            if (InventoryUI.Instance == null) return;

            // Swap items in inventory
            Item temp = InventoryUI.Instance.inventoryItems[index];
            InventoryUI.Instance.inventoryItems[index] = InventoryUI.Instance.inventoryItems[target.index];
            InventoryUI.Instance.inventoryItems[target.index] = temp;

            // Update the UI images
            InventoryUI.Instance.UpdateSlot(index);
            InventoryUI.Instance.UpdateSlot(target.index);
            return;
        }


        if (slotType == SlotType.Hotbar && target.slotType == SlotType.Inventory)
        {
            InventoryUI.Instance.inventoryItems[target.index] = ScriptableObject.Instantiate(draggedItem);
            InventoryUI.Instance.UpdateSlot(target.index);

            HotBarManager.Instance.slotItems[index] = null;
            HotBarManager.Instance.UpdateSlot(index);
            return;
        }




        if (slotType == SlotType.Inventory && target.slotType == SlotType.Hotbar)
        {
            Item item = InventoryUI.Instance.inventoryItems[index];
            if (item == null) { return; }

            // Instantiate a new copy for the hotbar
            HotBarManager.Instance.slotItems[target.index] = ScriptableObject.Instantiate(item);
            HotBarManager.Instance.UpdateSlot(target.index);

            InventoryUI.Instance.inventoryItems[index] = null;
            InventoryUI.Instance.UpdateSlot(index);
            return;
        }



    }





}

