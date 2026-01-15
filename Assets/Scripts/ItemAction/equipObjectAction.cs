using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Actions/equipObject")]
public class equipObjectAction : ItemAction
{
    private GameObject ItemAnimator;
    private Transform positionalTransform;
    public GameObject newObject;
    public GameObject objectToSpawn;
    public string positionalGameObjectName;
    public bool swingable = false;
    public bool throwable = false;
    public bool consumable = false;

    public override void Execute(Item item, GameObject gameManager)
    {
        Debug.Log("Made it here");
        if (newObject == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            ItemAnimator = player.transform.Find("ItemAnimator")?.gameObject;

            positionalTransform = FindChildRecursive(player.transform, positionalGameObjectName);
            if (positionalTransform == null || ItemAnimator == null) return;

            Debug.Log("Created Sword");
            newObject = Instantiate(objectToSpawn, positionalTransform);
            ItemAnimator.GetComponent<ItemEffect>().equippedItem = newObject;
            ItemAnimator.GetComponent<ItemEffect>().swingable = swingable;
            ItemAnimator.GetComponent<ItemEffect>().throwable = throwable;
            ItemAnimator.GetComponent<ItemEffect>().consumable = consumable;
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
