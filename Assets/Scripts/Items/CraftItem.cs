using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CraftItem : MonoBehaviour
{
    private SlotItem slotItem1;
    private SlotItem slotItem2;
    private SlotItem slotItem3;
    private SlotItem newItem;

    Item item1;
    Item item2;
    Item item3;

    public CraftingUI craftingUI;
    private bool superCrafting = false;
    
    public void craftItem()
    {
        slotItem1 = craftingUI.craftingItems[0];
        slotItem2 = craftingUI.craftingItems[1];

        if (slotItem1 == null || slotItem2 == null) return;
        
        Item item1 = slotItem1.itemDetails;
        Item item2 = slotItem2.itemDetails;

        if (craftingUI.isSuperCrafting())
        {
            Debug.Log("SuperCrafting");
            slotItem3 = craftingUI.craftingItems[2];
            Item item3 = slotItem3.itemDetails;
            superCrafting = true;
        }
        else
        {
            Debug.Log("NormalCrafting");
            superCrafting = false;
        }

        foreach (Item parent in item1.parentItems)
        {
            bool craftSuccess = true;
            if (item2.parentItems.Contains(parent) && (!superCrafting || item3.parentItems.Contains(parent)))
            {
                Debug.Log(parent.itemName);
                List<ItemGroup> requiredChildren = parent.childrenItems;
                foreach (ItemGroup group in requiredChildren)
                {
                    /*
                    Debug.Log("Passed 0");
                    Debug.Log(group.items[0].itemName);
                    Debug.Log(item1.itemName);
                    Debug.Log(group.items[0].itemName != item1.itemName);
                    Debug.Log(group.items[1].itemName);
                    Debug.Log(item2.itemName);
                    Debug.Log(group.items[1].itemName != item2.itemName);
                    Debug.Log(superCrafting && group.items[2].itemName != item3.itemName);
                    if (group.items[0].itemName != item1.itemName || group.items[1].itemName != item2.itemName || (superCrafting && group.items[2].itemName != item3.itemName)) continue;
                    Debug.Log("Passed 1");

                    if (group.itemQuantities[0] > slotItem1.quantity || group.itemQuantities[1] > slotItem2.quantity || (superCrafting && group.itemQuantities[2] > slotItem3.quantity)) continue;
                    Debug.Log("Passed 2");

                    slotItem1.quantity -= group.itemQuantities[0];
                    slotItem2.quantity -= group.itemQuantities[1];
                    slotItem3.quantity = (superCrafting) ? slotItem3.quantity - group.itemQuantities[2] : 0;
                    Debug.Log("Successful Crafting, crafted: " + parent.itemName);
                    newItem = new SlotItem(parent, 1);
                    craftingUI.craftingItems[3] = newItem;
                    craftingUI.UpdateSlot(3);
                    return;*/

                    List<string> itemNames = new List<string>();
                    int[] quantities = {0, 0, 0};
                    int numOfLoops = (superCrafting) ? 3 : 2;
                    for (int i=0; i<numOfLoops; i++)
                    {
                        Item requiredItem = group.items[i];

                        SlotItem correctItem = null;
                        if (requiredItem == null && !superCrafting) continue;
                        if (requiredItem.itemName == item1.itemName)
                        {
                            correctItem = slotItem1;
                            quantities[0] = group.itemQuantities[i];
                        }
                        else if (requiredItem.itemName == item2.itemName)
                        {
                            correctItem = slotItem2;
                            quantities[1] = group.itemQuantities[i];
                        }
                        else if (superCrafting && requiredItem.itemName == item3.itemName)
                        {
                            correctItem = slotItem3;
                            quantities[2] = group.itemQuantities[i];
                        }
                        else
                        {
                            break;
                        }
                        Debug.Log(correctItem.itemDetails.itemName);
                        Debug.Log("Made it here 3");
                        itemNames.Add(requiredItem.itemName);

                        if (correctItem.quantity < group.itemQuantities[i]) break;
                    }

                    if (itemNames.Contains(item1.itemName) && itemNames.Contains(item2.itemName) && (!superCrafting || itemNames.Contains(item3.itemName)))
                    {
                        if (craftingUI.craftingItems[3] != null && craftingUI.craftingItems[3].itemDetails.itemName == parent.itemName)
                        {
                            craftingUI.craftingItems[3].quantity += 1;
                        }
                        else if (craftingUI.craftingItems[3] != null)
                        {
                            Debug.Log("Another item is occupying result slot!");
                            return;
                        }
                        else
                        {
                            newItem = new SlotItem(parent, 1);
                            craftingUI.craftingItems[3] = newItem;
                            craftingUI.UpdateSlot(3);
                        }
                        slotItem1.quantity -= quantities[0];
                        slotItem2.quantity -= quantities[1];
                        if (superCrafting) slotItem3.quantity -= quantities[2];
                        if (slotItem1.quantity <= 0) craftingUI.ClearSlot(0);
                        if (slotItem2.quantity <= 0) craftingUI.ClearSlot(1);
                        if (superCrafting && slotItem3.quantity <= 0) craftingUI.ClearSlot(2);
                        craftingUI.UpdateSlot(0);
                        craftingUI.UpdateSlot(1);
                        craftingUI.UpdateSlot(2);
                        Debug.Log("Successful Crafting, crafted: " + parent.itemName);
                        return;
                    }
                }
            }
        }
        Debug.Log("Failed Crafting");
        return;
    }

}
