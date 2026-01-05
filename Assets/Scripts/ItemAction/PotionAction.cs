using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PotionType
{
    Heal,
    Hunger
}

[CreateAssetMenu(menuName = "Item System/Actions/Potion")]
public class PotionAction : ItemAction
{
    public PotionType potionType = PotionType.Heal;

    public int amount = 25;

    public override void Execute(Item item, GameObject gameManager)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("PotionAction: No object with tag 'Player'.");
            return;
        }

        // Find PlayerStats
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats = null)
        {
            stats = player.GetComponentInParent<PlayerStats>();
        }

        if (stats == null)
        {
            Debug.LogWarning("PotionAction: PlayerStats not found on Player.");
            return;
        }

        switch (potionType)
        {
            case PotionType.Heal:
                stats.Heal(amount);
                Debug.Log($"Used potion: {item.itemName} -> Heal {amount}");
                break;

            case PotionType.Hunger:
                stats.Eat(amount);
                Debug.Log($"Used potion: {item.itemName} -> Hunger +{amount}");
                break;
        }

        //if (potionType == PotionType.Heal)
        //{
        //    stats.Heal(amount);
        //    Debug.Log($"Used potion: {item.itemName} -> Heal {amount}");
        //}
        //else if (potionType == PotionType.Hunger)
        //{
        //    stats.Eat(amount);
        //    Debug.Log($"Used potionL {item.itemName} -> Hunger +{amount}");
        //}
    }
}
