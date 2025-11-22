using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items")]
public class SphereItem : Item
{
    public override void Use()
    {
        Debug.Log("Item");
    }
}
