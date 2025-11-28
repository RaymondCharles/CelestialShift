using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HotBarManager : MonoBehaviour
{
    public Button[] Slots;
    public Sprite emptySlotSprite;
    public static HotBarManager Instance;

    public Item[] slotItems;
    public int selectedSlot = 0;


    private void Awake()
    {
        // Set singleton instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize slots
        slotItems = new Item[Slots.Length];
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i].image.sprite = emptySlotSprite;
        }
    }

    //public void AddItemToSlot(int slotIndex, Item item)
    //{
    //    if (slotIndex < 0 || slotIndex >= Slots.Length) return;

    //    Slots[slotIndex].image.sprite = item.itemImg;
    //    slotItems[slotIndex] = item;
    //}
    //public void AddItemToSlot(Item item, int slotIndex = -1)
    //{

    //    if (slotIndex < 0)
    //    {
    //        slotIndex = GetFirstEmptySlot();
    //        if (slotIndex == -1)
    //        {
    //            Debug.Log("No empty slots!");
    //            return;
    //        }
    //    }


    //    Slots[slotIndex].image.sprite = item.itemImg;
    //    slotItems[slotIndex] = item;
    //}
    public void AddItemToSlot(Item item, int slotIndex = -1)
    {
        if (item == null) return;

        if (slotIndex < 0)
        {
            slotIndex = GetFirstEmptySlot();
            if (slotIndex == -1)
            {
                Debug.Log("No empty slots!");
                return;
            }
        }

        // CREATE A NEW INSTANCE FOR THIS SLOT
        Item newItem = ScriptableObject.Instantiate(item);

        Slots[slotIndex].image.sprite = newItem.itemImg;
        slotItems[slotIndex] = newItem;
    }



    public void ClearSlot(Item item)
    {
        for (int i = 0; i < slotItems.Length; i++)
        {
            if (slotItems[i] == item)
            {
                Slots[i].image.sprite = emptySlotSprite;
                slotItems[i] = null;
                break;
            }
        }
    }
    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < slotItems.Length; i++)
        {
            if (slotItems[i] == null)
                return i;
        }
        return -1;
    }

    public void SlotSwap(int slot1, int slot2)
    {
        Item temp = slotItems[slot1];
        slotItems[slot1] = slotItems[slot2];
        slotItems[slot2] = temp;

        Slots[slot1].image.sprite = slotItems[slot1] != null ? slotItems[slot1].itemImg : emptySlotSprite;
        Slots[slot2].image.sprite = slotItems[slot2] != null ? slotItems[slot2].itemImg : emptySlotSprite;
        UpdateSelectedItem();

    }

    public void UpdateSelectedItem()
    {
        for (int i = 0; i < slotItems.Length; i++)
        {
            if (slotItems[i] != null)
            {
                slotItems[i].selected = false;
            }
        }
        Item selectedItem = slotItems[selectedSlot];
        if (selectedItem != null) { 
            selectedItem.selected = true;
        }
    }

    public void UpdateSlot(int slot)
    {
        Slots[slot].image.sprite = slotItems[slot] != null ? slotItems[slot].itemImg : emptySlotSprite;
    }

    public void DropSelectedItem()
    {
        Item item = slotItems[selectedSlot];
        if (item == null) return;

        PlayerMotion player = PlayerMotion.Instance;
        if (player == null) return;

        Vector3 dropPos = player.playerTransform.position + player.playerTransform.forward;


        dropPos.y = player.playerTransform.position.y;

        Inventory.Instance.GenerateItem(item, dropPos);

        // Remove from hotbar
        ClearSlot(item);
    }

}
