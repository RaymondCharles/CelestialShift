using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public List<Item> items = new List<Item>();

  

    public void Awake()
    {
        Instance = this;
    }

    public void addItem(Item item, Vector3 pos)
    {
        if (!items.Contains(item))
        {
            item.quantity = 1;
            items.Add(item);
        }
        else
        {
            item.quantity += 1;
            if (item.quantity > item.quantityLimit)
            {
                item.quantity -= 1;
                GenerateItem(item, pos);
            }
        }
    }

    public void DropItem(Item item, Vector3 pos)
    {
        if (!items.Contains(item))
        {
            return;
        }

        item.quantity--;

        if (item.quantity <= 0)
        {
            items.Remove(item);
        }
        GenerateItem(item, pos);
    }



    public void GenerateItem(Item item, Vector3 pos)
    {
        Instantiate(item.worldPrefab, pos, Quaternion.identity);
    }


}
