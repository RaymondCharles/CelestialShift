using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTemp : MonoBehaviour
{
    public Item[] allItems;
    public GameObject player;

    public  Dictionary<Vector2Int, List<GameObject>> globalEnemyDict;
    private int Level = 10;

    public int GetLevel(){
        return Level;
    }
    public EndlessTerrain endlessTerrain;
    

    private void Awake()
    {
        globalEnemyDict = new Dictionary<Vector2Int, List<GameObject>>();
        endlessTerrain = GetComponent<EndlessTerrain>();
    }

    private void Update()
    {   
        Ray ray = new Ray(player.transform.position, Vector3.down);
        EndlessTerrain.TerrainChunk terrainChunk = null;
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            if (hit.collider.name == "Terrain Chunk")
            {
                const float scale = 5f;
                int chunkSize = endlessTerrain.chunkSize - 1; // 240 if mapChunkSize = 241
                float newHitX = hit.point.x + (0.5f * (chunkSize * scale));
                float newHitZ = hit.point.z + (0.5f * (chunkSize * scale));
                // world -> scaled terrain space
                Vector2 hitScaled = new Vector2(newHitX, newHitZ) / scale;

                // scaled -> chunk coord (dictionary key)
                Vector2Int chunkCoord = new Vector2Int(
                    Mathf.FloorToInt(hitScaled.x / chunkSize),
                    Mathf.FloorToInt(hitScaled.y / chunkSize)
                );

                // lookup the correct TerrainChunk
                if (!endlessTerrain.terrainChunkDictionary.TryGetValue(chunkCoord, out terrainChunk))
                {
                    Debug.LogWarning($"No chunk found for {chunkCoord}");
                    return;
                }
                if (terrainChunk.enemyCount < Level){//FOR NOW WE SPAWN UNTIL LEVEL IS FULFILLED - add a 30 second wait after killing enemy to remove from list
                // Enemies will raycast every frame to find chunk parent, add self to new list, remove from old one. 
                // This way enemies will despawn if player moves far enough away
                // enemies can chase until limit of 100 or whatever we decide and will just be added to new terrain chunk list
                // new enemies will only be spawned in a terrain chunk if the current chunk enemy count is less than 100
                // enemy spawning per chunk is tied to count
                globalEnemyDict.Add(terrainChunk.coord, terrainChunk.spawnEnemies(Level - terrainChunk.enemyCount));
                }
            }
        }
    }
}
