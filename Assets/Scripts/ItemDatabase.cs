using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    public List<Item> allItems;

    void Awake()
    {
        Instance = this;
    }

    public Item GetItemByName(string itemName)
    {
        return allItems.Find(i => i.itemName == itemName);
    }
}
