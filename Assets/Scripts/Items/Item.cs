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
    public bool usable;
    public string itemID;

    public int quantityLimit;

    public bool selected;

    public List<Item> parentItems;

    public List<ItemGroup> childrenItems;

    public GameObject worldPrefab;

    public ItemAction action;
    public GameObject minimapIconPrefab;
    private GameObject ItemAnimator;
    private Transform positionalTransform;
    public GameObject newObject;
    public GameObject objectToSpawn;
    public string positionalGameObjectName;
    public bool swingable = false;
    public bool throwable = false;
    public bool consumable = false;

    public void Equip()
    {
        if (newObject == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            ItemAnimator = player.transform.Find("ItemAnimator")?.gameObject;

            positionalTransform = FindChildRecursive(player.transform, positionalGameObjectName);
            if (positionalTransform == null || ItemAnimator == null) return;

            newObject = Instantiate(objectToSpawn, positionalTransform);
            ItemAnimator.GetComponent<ItemEffect>().equippedItem = newObject;
            ItemAnimator.GetComponent<ItemEffect>().swingable = swingable;
            ItemAnimator.GetComponent<ItemEffect>().throwable = throwable;
            ItemAnimator.GetComponent<ItemEffect>().consumable = consumable;
        }
    }

    public void UnEquip()
    {
        if (newObject!=null) Destroy(newObject);
    }

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


    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}

[System.Serializable]
public class ItemGroup
{
    public Item[] items = new Item[3];
    public int[] itemQuantities = new int[3];
}