using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInstance : MonoBehaviour
{
    public Item item;
    public int quantity = 0;
    public HotBarManager hotBarManager;
 

    [HideInInspector]
    public GameObject minimapIconPrefab; 

    private void Awake()
    {
        if (item != null)
            minimapIconPrefab = item.minimapIconPrefab;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (HotBarManager.Instance != null)
                quantity = HotBarManager.Instance.AddItemToSlot(item, quantity);

            if (Inventory.Instance != null && quantity > 0)
                quantity = Inventory.Instance.addItem(item, quantity);

            if (quantity <= 0)
                Destroy(gameObject);
        }
    }
}
