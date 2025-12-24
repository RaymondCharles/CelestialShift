using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 450;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInVD;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunksLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        chunkSize = mapGenerator.mapChunkSize - 1;
        // calculate how many chunks are visible in view distance
        chunksVisibleInVD = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks(){

        for (int i = 0; i < visibleTerrainChunksLastUpdate.Count; i++){
            visibleTerrainChunksLastUpdate[i].SetVisible(false);
        }
        visibleTerrainChunksLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);    
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        // loop through visible chunks
        for (int yOffset = -chunksVisibleInVD; yOffset <= chunksVisibleInVD; yOffset++){
            for (int xOffset = -chunksVisibleInVD; xOffset <= chunksVisibleInVD; xOffset++){
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                // using viewedChunkCoord, check if chunk exists in dictionary, else, create it
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)){
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible()){
                        visibleTerrainChunksLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                } else {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
                }
            }
        }
    }

    public class TerrainChunk {
        // constructs a terrain chunk at given coord with given size
        GameObject meshObject;
        public Vector2 position;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent){
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            //create mesh object, set 3d position and scale
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false); // default to not visible
        }

        public void UpdateTerrainChunk(){
            // determine if chunk is visible based on viewer position, visible true if within maxViewDistance
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible){
            // sets object to visible or not
            meshObject.SetActive(visible);
        }
        public bool IsVisible(){
            return meshObject.activeSelf;
        }
    }
}
