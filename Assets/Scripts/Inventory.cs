using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public List<SlotItem> items = new List<SlotItem>();
    public HotBarManager hotBarManager;

  

    public void Awake()
    {
        Instance = this;
    }

    public void addItem(Item item, int quantity, Vector3 pos)
    {
        int leftOver;
        foreach (SlotItem itemInList in items)
        {
            if (itemInList.itemDetails.itemName == item.itemName)
            {
                itemInList.quantity += quantity;
                leftOver = itemInList.quantity - item.quantityLimit;
                if (leftOver > 0)
                {
                    itemInList.quantity -= leftOver;
                    GenerateItem(item, leftOver, pos);
                }
                return;
            }
        }
        leftOver = quantity - item.quantityLimit;
        if (leftOver > 0)
        {
            items.Add(new SlotItem(item, item.quantityLimit));
            GenerateItem(item, leftOver, pos);
        }
    }

    public void DropItem(SlotItem slotItem, Vector3 pos)
    {
        slotItem.quantity--;

        if (slotItem.quantity <= 0)
        {
            items.Remove(slotItem);
        }

        if (slotItem.itemDetails.worldPrefab != null)
            GenerateItem(slotItem.itemDetails, slotItem.quantity, pos);
    }



    public void GenerateItem(Item item, int quantity, Vector3 pos)
    {
        GameObject newItem = Instantiate(item.worldPrefab, pos, Quaternion.identity);
        newItem.GetComponent<ItemInstance>().hotBarManager = hotBarManager;
        newItem.GetComponent<ItemInstance>().quantity = quantity;
    }


}
