using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Actions/Sword")]
public class swordAction : ItemAction
{
    private GameObject ItemAnimator;
    private Transform rightHandEnd;
    public GameObject sword;
    public GameObject swordToSpawn;

    public override void Execute(Item item, GameObject gameManager)
    {
        Debug.Log("Made it here");
        if (sword == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            ItemAnimator = player.transform.Find("ItemAnimator")?.gameObject;

            rightHandEnd = FindChildRecursive(player.transform, "right hand_end");
            Debug.Log(ItemAnimator == null);
            Debug.Log(rightHandEnd == null);
            if (rightHandEnd == null || ItemAnimator == null) return;

            Debug.Log("Created Sword");
            sword = Instantiate(swordToSpawn, rightHandEnd);
            ItemAnimator.GetComponent<ItemEffect>().equippedItem = sword;
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
