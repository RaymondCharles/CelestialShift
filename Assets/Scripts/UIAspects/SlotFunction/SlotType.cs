using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SlotType
    {
        Hotbar,
        Inventory,
        Crafting
    }

public class SlotItem
{
    public Item itemDetails;
    public int quantity = 0;
    public bool selected;

    public SlotItem(Item itemDetails, int quantity)
    {
        this.itemDetails = itemDetails;
        this.quantity = quantity;
    }
}