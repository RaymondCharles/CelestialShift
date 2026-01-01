using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HotBarManager : MonoBehaviour, UIManager
{
    public Button[] Slots;
    public Sprite emptySlotSprite;
    public static HotBarManager Instance;

    public SlotItem[] slotItems;
    public int selectedSlot = -1;


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
        slotItems = new SlotItem[Slots.Length];
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
    public int AddItemToSlot(Item item, int quantity, int slotIndex = -1)
    {

        int leftOver = quantity;
        int index = 0;
        foreach (SlotItem item2 in slotItems)
        {
            Debug.Log(index);
            if (item2==null) continue;
            if (item2.itemDetails.itemName == item.itemName)
            {
                Debug.Log("Added quantity, was " + item2.quantity.ToString() + "and now is " + (item2.quantity + quantity).ToString());
                item2.quantity+=quantity;
                leftOver = item2.quantity - item.quantityLimit;
                if (leftOver > 0)
                {
                    item2.quantity -= leftOver;
                }
                else if (leftOver <= 0)
                {
                    return 0;
                }
                index++;
            }
        }
        
        if (slotIndex < 0)
        {
            slotIndex = GetFirstEmptySlot();
            if (slotIndex == -1)
            {
                Debug.Log("No empty slots!");
                return leftOver;
            }
        }

        while ((slotIndex = GetFirstEmptySlot()) != -1 && leftOver > item.quantityLimit)
        {
            newInstance(item.quantityLimit);
            leftOver -= item.quantityLimit;
        }

        if ((slotIndex = GetFirstEmptySlot()) != -1 && leftOver > 0)
        {
            newInstance(leftOver);
            return 0;
        }
        else
        {
            return leftOver;
        }

        void newInstance(int quantity)
        {
            // CREATE A NEW INSTANCE FOR THIS SLOT
            Item newItem = ScriptableObject.Instantiate(item);

            Slots[slotIndex].image.sprite = newItem.itemImg;
            slotItems[slotIndex] = new SlotItem(newItem, quantity);
            UpdateSlot(slotIndex);
        }
    }



    public void ClearSlot(SlotItem slotItem)
    {
        for (int i = 0; i < slotItems.Length; i++)
        {
            if (slotItems[i] == slotItem)
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
        SlotItem temp = slotItems[slot1];
        slotItems[slot1] = slotItems[slot2];
        slotItems[slot2] = temp;

        Slots[slot1].image.sprite = slotItems[slot1] != null ? slotItems[slot1].itemDetails.itemImg : emptySlotSprite;
        Slots[slot2].image.sprite = slotItems[slot2] != null ? slotItems[slot2].itemDetails.itemImg : emptySlotSprite;
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
        SlotItem selectedItem = slotItems[selectedSlot];
        if (selectedItem != null) { 
            selectedItem.selected = true;
        }
    }

    public void UpdateSlot(int slotIndex)
    {
        Slots[slotIndex].image.sprite = slotItems[slotIndex] != null ? slotItems[slotIndex].itemDetails.itemImg : emptySlotSprite;
    }

    public void DropSelectedItem()
    {
        SlotItem slotItem = slotItems[selectedSlot];
        if (slotItem == null) return;

        PlayerMotion player = PlayerMotion.Instance;
        if (player == null) return;

        Vector3 dropPos = player.playerTransform.position + player.playerTransform.forward;


        dropPos.y = player.playerTransform.position.y;

        Inventory.Instance.GenerateItem(slotItem.itemDetails, slotItem.quantity, dropPos);

        // Remove from hotbar
        ClearSlot(slotItem);
    }

    public SlotItem GetItem(int slotIndex)
    {
        return slotItems[slotIndex];
    }

    public void SetItem(int slotIndex, SlotItem item)
    {
        slotItems[slotIndex] = item;
        return;
    }

}
