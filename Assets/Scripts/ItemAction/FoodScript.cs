using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Actions/Eat")]
public class FoodScript : ItemAction
{
    public int amount;


    public override void Execute(Item item, GameObject gameManager)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerStats>().Eat(amount);
    }
}