using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInstance : MonoBehaviour
{
    public Item item;
    public HotBarManager hotBarManager;
    public int quantity = 0;




    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (HotBarManager.Instance != null) quantity = HotBarManager.Instance.AddItemToSlot(item, quantity);
            if (Inventory.Instance != null && quantity > 0) quantity = Inventory.Instance.addItem(item, quantity);
            if (quantity <= 0) Destroy(gameObject);
            else
            {
                Debug.Log("Item picked Up" + item.itemName);
            }
        
        }
    }


}
