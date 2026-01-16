using System.Collections.Generic;
using UnityEngine;

public class GPUInstancedGrassRenderer : MonoBehaviour
{
    [SerializeField] Mesh grassMesh;
    [SerializeField] Material grassMaterial;
    [SerializeField] float spacing = 0.4f;
    [SerializeField] float grassScale = 300.0f;

    const int MAX_INSTANCES = 1023;

    // Cache: chunkId -> batches -> matrices
    private readonly Dictionary<Vector2Int, List<Matrix4x4[]>> chunkBatches = new();

    void Awake()
    {
        if (grassMaterial != null)
            grassMaterial.enableInstancing = true;
    }

    void Update()
    {
        // Draw all cached chunks (you’ll likely filter to "visible chunks" here)
        foreach (var kvp in chunkBatches)
        {
            var batches = kvp.Value;
            for (int i = 0; i < batches.Count; i++)
            {
                Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, batches[i]);
            }
        }
    }

    // called upon chunk generation, creates batches and stores in cache
    public void BuildGrassForChunk(Vector2Int chunkId, float scale, Vector2 chunkPositionMapSpace, Bounds chunkBounds, mapGenerator mapGenerator, MapData mapData)
    {
        if (grassMesh == null || grassMaterial == null || mapData.noiseMap == null || mapData.biomeGenData.voronoiMap == null)
            return;

        int width = mapData.noiseMap.GetLength(0);
        int height = mapData.noiseMap.GetLength(1);

        if (mapData.biomeGenData.voronoiMap.GetLength(0) != width || mapData.biomeGenData.voronoiMap.GetLength(1) != height)
        {
            Debug.LogError($"Chunk {chunkId}: Grass generation failed due to biomeMap size mismatch.");
            return;
        }

        // Build matrices in a List first, then freeze into arrays of <=1023
        var current = new List<Matrix4x4>(MAX_INSTANCES);
        var batches = new List<Matrix4x4[]>();
        int chunkSize = mapData.chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {   
                BiomeScriptableObject biome = mapData.biomeDict[mapData.biomeGenData.voronoiMap[x, y].getBiome()];
                if (biome.name != "Grass Plains"){continue;}
 
                float z = mapData.heightCurve.Evaluate(mapData.noiseMap[x, y]) * (mapGenerator.meshHeightMultiplier * biome.biomeHeightMultiplier);

                float worldX = x + chunkPositionMapSpace.x - 0.5f * chunkSize;
                float worldY = chunkPositionMapSpace.y + ((0.5f * chunkSize) - y);

                Vector3 worldPos = new Vector3(worldX * scale, z * scale, worldY * scale);

                float grassScale = 1f;
                current.Add(Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one * grassScale));

                // unscaled grass
                //current.Add(Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one));

                if (current.Count == MAX_INSTANCES)
                {
                    batches.Add(current.ToArray());
                    current.Clear();
                }
            }
        }

        if (current.Count > 0)
            batches.Add(current.ToArray());

        // Replace cache for this chunk
        chunkBatches[chunkId] = batches;
    }

    // Call when chunk unloads/despawns
    public void RemoveChunk(Vector2Int chunkId)
    {
        chunkBatches.Remove(chunkId);
    }
}