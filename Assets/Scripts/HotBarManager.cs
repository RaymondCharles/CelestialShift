using UnityEngine;
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
    public void AddItemToSlot(Item item, int slotIndex = -1)
    {
        // If no slotIndex is provided, find the first empty slot
        if (slotIndex < 0)
        {
            slotIndex = GetFirstEmptySlot();
            if (slotIndex == -1)
            {
                Debug.Log("No empty slots!");
                return;
            }
        }

   
        Slots[slotIndex].image.sprite = item.itemImg;
        slotItems[slotIndex] = item;
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
                return i; // return first empty slot
        }
        return -1; // no empty slots
    }

}
