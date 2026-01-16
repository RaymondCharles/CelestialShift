using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    public List<Item> allItems;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Item GetItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return null;

        if (allItems == null)
        {
            Debug.LogError("ItemDatabase: allItems list is NULL!");
            return null;
        }

        return allItems.Find(i => i != null && i.itemName == itemName);
    }
}
