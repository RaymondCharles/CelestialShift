using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item System/Actions/Spawn")]
public class SpawnAction : ItemAction
{
    public GameObject prefabToSpawn;

    public override void Execute(Item item, GameObject gameManager)
    {
        if (prefabToSpawn != null)
        {
            Object.Instantiate(prefabToSpawn, gameManager.GetComponent<GameManagerTemp>().player.transform.position, gameManager.GetComponent<GameManagerTemp>().player.transform.rotation);
            Debug.Log($"Spawned {item.itemName} in the world!");
        }
    }
}