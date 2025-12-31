using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTemp : MonoBehaviour
{
    public Item[] allItems;
    public GameObject player;

    void Awake()
    {
        foreach (Item item in allItems)
        {
            item.InitializeDictionary();
        }
    }
}
