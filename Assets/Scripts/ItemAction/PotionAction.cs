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
        if (gameManager == null)
        {
            Debug.LogWarning("PotionAction: gameManager is null.");
            return;
        }

        // Use the same pattern as SpawnAction (no tags!)
        var gm = gameManager.GetComponent<GameManagerTemp>();
        if (gm == null || gm.player == null)
        {
            Debug.LogWarning("PotionAction: GameManagerTemp or player reference missing.");
            return;
        }

        // PlayerStats might be on player root or a child (common)
        PlayerStats stats = gm.player.GetComponent<PlayerStats>();
        if (stats == null) stats = gm.player.GetComponentInChildren<PlayerStats>();
        if (stats == null) stats = gm.player.GetComponentInParent<PlayerStats>();

        if (stats == null)
        {
            Debug.LogWarning("PotionAction: PlayerStats not found on GameManagerTemp.player hierarchy.");
            return;
        }

        switch (potionType)
        {
            case PotionType.Heal:
                stats.Heal(amount);
                Debug.Log($"Used potion: {item.itemName} -> Heal +{amount}");
                break;

            case PotionType.Hunger:
                stats.Eat(amount);
                Debug.Log($"Used potion: {item.itemName} -> Hunger +{amount}");
                break;
        }
    }
}
