using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    
    const float scale = 5f;
    const float chunkUpdateMoveThreshold = 25f; // distance viewer must move in order to update visible chunks
    const float sqrChunkUpdateMoveThreshold = chunkUpdateMoveThreshold * chunkUpdateMoveThreshold;

    public LODInfo[] detailLevels;
    public static float maxViewDistance;
    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    public static Vector2  previousViewerPosition;
    static mapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInVD;

    Dictionary<Vector2Int, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2Int, TerrainChunk>();
    static List<TerrainChunk> visibleTerrainChunksLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<mapGenerator>();
     
        maxViewDistance = detailLevels[detailLevels.Length -1].visibleDstThreshold;
        chunkSize = mapGenerator.mapChunkSize - 1;
        // calculate how many chunks are visible in view distance
        chunksVisibleInVD = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;
        if ((previousViewerPosition - viewerPosition).sqrMagnitude > sqrChunkUpdateMoveThreshold){
            previousViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks(){

        for (int i = 0; i < visibleTerrainChunksLastUpdate.Count; i++){
            visibleTerrainChunksLastUpdate[i].SetVisible(false);
        }
        visibleTerrainChunksLastUpdate.Clear();

        int currentChunkCoordX = Mathf.FloorToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.FloorToInt(viewerPosition.y / chunkSize);

        // loop through visible chunks
        for (int yOffset = -chunksVisibleInVD; yOffset <= chunksVisibleInVD; yOffset++){
            for (int xOffset = -chunksVisibleInVD; xOffset <= chunksVisibleInVD; xOffset++){
                Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                // using viewedChunkCoord, check if chunk exists in dictionary, else, create it
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)){
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                } else {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial, mapGenerator));
                }
            }
        }
    }

    public class TerrainChunk {
        // constructs a terrain chunk at given coord with given size
        GameObject meshObject;
        public Vector2Int coord;
        public Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        mapGenerator mapGenerator;
        LODInfo[] detailLevels;
        LODmesh[] lodMeshes;
        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2Int coord, int size, LODInfo[] detailLevels, Transform parent, Material material, mapGenerator mapGen){
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.mapGenerator = mapGen;

            // World origin (x,z)
            position = new Vector2(coord.x * size, coord.y * size);
            Vector3 positionV3 = new Vector3(position.x, 0f, position.y);

            // bounds must be in Z
            Vector3 boundsCenter = positionV3 + new Vector3(size * 0.5f, 0f, size * 0.5f);
            Vector3 boundsSize   = new Vector3(size, 10000f, size); // tall Y so height doesn't matter
            bounds = new Bounds(boundsCenter, boundsSize);
            

            //create mesh object, set 3d position and scale, add renderer, filter, collider
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false); // default to not visible

            // create LOD meshes for different detail levels
            lodMeshes = new LODmesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++){
                lodMeshes[i] = new LODmesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            // request map data
            Debug.Log($"Requesting map data for chunk coord {coord} at world origin {position}");

            Vector2 chunkOrigin = new Vector2(coord.x * size, coord.y * size);
            Vector2 chunkCentre = chunkOrigin + Vector2.one * (size * 0.5f);

            //mapGenerator.RequestMapData(chunkCentre, OnMapDataReceived);
            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }
        
        void OnMapDataReceived(MapData mapData){
            // request mesh data
            this.mapData = mapData;
            mapDataReceived = true;

            /*
            int size = mapGenerator.mapChunkSize;

            
            int size = mapGenerator.mapChunkSize;
            Color[] debugColourMap = new Color[size * size];
            
            // "Row" along X: darkest at x=0, lighter as we go outward
            float maxBands = 10f; // number of chunk-columns until near white
            float t = Mathf.Clamp01(Mathf.Abs(coord.x) / maxBands);
            Color c = Color.Lerp(Color.black, Color.white, t);

            for (int i = 0; i < debugColourMap.Length; i++){
                debugColourMap[i] = c;
            }
            

            // Horizontal bands: darkest at y=0, lighter outward
            float maxBands = 10f; // number of chunk-rows until near white
            float t = Mathf.Clamp01(Mathf.Abs(coord.y) / maxBands);
            Color c = Color.Lerp(Color.black, Color.white, t);
            for (int i = 0; i < debugColourMap.Length; i++){
                debugColourMap[i] = c;
            }
        
            for (int y = 0; y < size; y++){
                for (int x = 0; x < size; x++){
                    float u = x / (float)(size - 1);
                    float v = y / (float)(size - 1);
                    debugColourMap[y * size + x] = new Color(u, v, 0f, 1f);
                }
            }

            Texture2D texture = TextureGenerator.TextureFromColourMap(
                debugColourMap,
                size,
                size
            );
            

            // Flip the colour map vertically (y)
            Color[] flipped = new Color[size * size];
            for (int y = 0; y < size; y++){
                for (int x = 0; x < size; x++){
                    flipped[y * size + x] =
                        mapData.colourMap[(size - 1 - y) * size + x];
                }
            }

            Texture2D texture = TextureGenerator.TextureFromColourMap(flipped, size, size);
            */
            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, mapGenerator.mapChunkSize, mapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk(){
            // determine if chunk is visible based on viewer position, visible true if within maxViewDistance
            if (mapDataReceived){
                Vector3 viewerPos3 = new Vector3(viewerPosition.x, 0f, viewerPosition.y);
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos3));
                
                bool visible = viewerDstFromNearestEdge <= maxViewDistance;
                
                if (visible){
                    int lodIndex = 0;

                    // determine appropriate LOD index based on distance from viewer
                    for (int i = 0; i < detailLevels.Length -1; i++){
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold){
                            lodIndex = i + 1;
                        } else {
                            break;
                        }
                    }

                    // if map data received, check if LOD mesh has been generated, if so, assign to mesh filter, else request it
                    if (lodIndex != previousLODIndex){
                        LODmesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh){
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                            meshCollider.sharedMesh = lodMesh.mesh;
                        } else if (!lodMesh.hasRequestedMesh){
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    visibleTerrainChunksLastUpdate.Add(this);
                }
                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible){
            // sets object to visible or not
            meshObject.SetActive(visible);
        }
        public bool IsVisible(){
            return meshObject.activeSelf;
        }
    }

    class LODmesh{
        // level of detail mesh class to hold mesh data and whether it has been requested or generated
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODmesh(int levelOfDetail, System.Action updateCallback){
            lod = levelOfDetail;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData){
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        public void RequestMesh(MapData mapData){
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo{
        // level of detail info struct to hold LOD and visible distance threshold
        public int lod;
        public float visibleDstThreshold;

        public void LODinfo(int levelOfDetail, float visibleDistanceThreshold){
            lod = levelOfDetail;
            visibleDstThreshold = visibleDistanceThreshold;
        }
    }
}
