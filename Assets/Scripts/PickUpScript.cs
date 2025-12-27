using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpScript : MonoBehaviour
{
    public Item item;
    public HotBarManager hotBarManager;




    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        Inventory.Instance.addItem(item, transform.position);
    //        HotBarManager.Instance.AddItemToSlot(0, item);

    //        Destroy(gameObject);
    //        Debug.Log("Item picked Up");
    //    }
    //}
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            
            Inventory.Instance.addItem(item, transform.position);

            if (HotBarManager.Instance != null)
            {
                HotBarManager.Instance.AddItemToSlot(item); 
            }
            if (gameObject.name != "Well")
            {
                Destroy(gameObject);
            }
            Debug.Log("Item picked Up" + item.itemName);
        }
    }


}
