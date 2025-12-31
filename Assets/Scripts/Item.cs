using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
fileName = "NewItemData",
menuName = "New Item Data"
)]

public class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemImg;

    public int quantityLimit;

    public bool selected;

    public Item[] parentItems;
    public Item[] childrenItems;

    public GameObject worldPrefab;

    public ItemAction action;


    public void Use(GameObject gameManager)
    {
        Debug.Log("Trying to use");
        if (action!=null)
        {
            action.Execute(this, gameManager);
        }
        else
        {
            Debug.Log("Did not execute item action");
        }
    }
}
