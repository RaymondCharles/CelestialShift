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
            Inventory.Instance.addItem(item, quantity, transform.position);

            if (HotBarManager.Instance != null)
            {
                HotBarManager.Instance.AddItemToSlot(item, quantity); 
            }
            Debug.Log("Item picked Up" + item.itemName);
            Destroy(gameObject);
        }
    }


}
