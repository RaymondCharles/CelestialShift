using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class equippedItem : MonoBehaviour
{

    public Item item;
    // Update is called once per frame
    void Update()
    {
        if (HotBarManager.Instance.selectedSlot == -1 || HotBarManager.Instance.slotItems[HotBarManager.Instance.selectedSlot] == null || HotBarManager.Instance.slotItems[HotBarManager.Instance.selectedSlot].itemDetails.itemName!=item.itemName)
        {
            Destroy(this.gameObject);
            return;
        }
    }
}
