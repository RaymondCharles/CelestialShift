using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArmorUI : MonoBehaviour
{
    public Image[] slots;
    public SlotItem[] armorItem;
    public static ArmorUI Instance;
    public Sprite emptySprite;
    public ItemEffect itemEffect;

    private void Awake()
    {
        Instance = this;
        armorItem = new SlotItem[1];
    }


    public void UpdateSlot(int slotIndex)
    {
        slots[slotIndex].sprite = armorItem[slotIndex] != null ? armorItem[slotIndex].itemDetails.itemImg : emptySprite;
        if (armorItem[slotIndex] == null)
        {
            itemEffect.UpdatePlayerArmor(null);
        }
        else
        {
            itemEffect.UpdatePlayerArmor(armorItem[0].itemDetails);
        }
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= armorItem.Length) return;

        armorItem[index] = null;
        UpdateSlot(index);
        itemEffect.UpdatePlayerArmor(null);
    }

    public SlotItem GetArmor()
    {
        return armorItem[0];
    }

    public void SetItem(int slotIndex, SlotItem item)
    {
        armorItem[slotIndex] = item;
        return;
    }

}
