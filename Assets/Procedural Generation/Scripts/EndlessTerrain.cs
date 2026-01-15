using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public int chunkSize;
    int chunksVisibleInVD;

    public Dictionary<Vector2Int, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2Int, TerrainChunk>();
    static List<TerrainChunk> visibleTerrainChunksLastUpdate = new List<TerrainChunk>();

    public static EndlessTerrain Instance;

    private Vector2Int _currentPlayerChunk = new Vector2Int (0,0);
    private TerrainChunk currentPlayerChunkRef;
    
    
    void Awake() => Instance = this;

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
        LoadTerrain();
    }
    public void LoadTerrain()
    {
        // 1) Viewer world position (in Unity units)
        Vector3 wpos = viewer.position;

        // 2) Convert world -> "scaled terrain space" the same way as CheckBiome()
        //    CheckBiome does:
        //      newHitX = hit.point.x + 0.5*(chunkSize*scale)
        //      hitScaled = new Vector2(newHitX, newHitZ) / scale
        //    Then floors hitScaled/chunkSize.
        float halfChunkWorld = 0.5f * (chunkSize * scale);

        float scaledX = (wpos.x + halfChunkWorld) / scale;
        float scaledY = (wpos.z + halfChunkWorld) / scale; // use z like CheckBiome

        // 3) Now convert scaled -> chunk coord (dictionary key)
        Vector2Int playerChunkCoord = new Vector2Int(
            Mathf.FloorToInt(scaledX / chunkSize),
            Mathf.FloorToInt(scaledY / chunkSize)
        );

        // (Optional) keep viewerPosition if you use it elsewhere for visibility logic
        viewerPosition = new Vector2(scaledX, scaledY);

        // 4) Detect entering a new chunk
        if (playerChunkCoord != _currentPlayerChunk)
        {
            _currentPlayerChunk = playerChunkCoord;
            Debug.Log($"changed chunks -> {playerChunkCoord}");
            
            if (terrainChunkDictionary.TryGetValue(playerChunkCoord, out var chunk))
            {
                // Promote navmesh detail ONLY for the chunk the player is on (your choice)
                // chunk.SetNavMeshLodOverride(0);

                currentPlayerChunkRef = chunk;
                if (chunk.navMeshLodIndex != 0)
                {
                    chunk.navMeshLodIndex = 0;
                    chunk.navQueued = false;
                    chunk.UpdateTerrainChunk();
                }
            }
            else
            {
                // This can happen for 1 frame if the chunk isn't created yet.
                // It will be handled once UpdateVisibleChunks creates it.
                currentPlayerChunkRef = null;
            }
        }


        // 5) Update visible chunks only when moved enough (uses viewerPosition in scaled space)
        if ((previousViewerPosition - viewerPosition).sqrMagnitude > sqrChunkUpdateMoveThreshold)
        {
            previousViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }

/*
    public void LoadTerrain()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        int cx = Mathf.FloorToInt(viewerPosition.x / chunkSize);
        int cy = Mathf.FloorToInt(viewerPosition.y / chunkSize);
        Vector2Int playerChunkCoord = new Vector2Int(cx, cy);

        if (playerChunkCoord != _currentPlayerChunk)
        { 
            _currentPlayerChunk = playerChunkCoord;
            Debug.Log("changed chunks");
            if (terrainChunkDictionary.TryGetValue(playerChunkCoord, out var chunk))
            {
                Debug.Log(playerChunkCoord);
                //chunk.SetNavMeshLodOverride(0);
                currentPlayerChunkRef = chunk;
            }
        }

        
        if ((previousViewerPosition - viewerPosition).sqrMagnitude > sqrChunkUpdateMoveThreshold){
            previousViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }
    }*/

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
        public MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        List<GameObject> dungeonList = new List<GameObject>();
        List<GameObject> treeList = new List<GameObject>();
        NavMeshSurface surface;

        public bool navQueued = false;
        GameObject navSourceObject;
        MeshFilter navSourceFilter;
        MeshCollider navSourceCollider;
        public int navMeshLodIndex; // which lod to use for navmesh
        int prevNMLodIndex;


        
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
            meshObject.layer = LayerMask.NameToLayer("Ground");
            




            navSourceObject = new GameObject("NavMeshSource");
            navSourceObject.transform.parent = meshObject.transform;
            navSourceObject.transform.localPosition = Vector3.zero;
            navSourceObject.transform.localRotation = Quaternion.identity;
            navSourceObject.transform.localScale = Vector3.one; // IMPORTANT: do not scale this one

            navSourceObject.layer = LayerMask.NameToLayer("NavMeshOnly");

            navSourceFilter = navSourceObject.AddComponent<MeshFilter>();
            navSourceCollider = navSourceObject.AddComponent<MeshCollider>();

            // NavMeshSurface should ONLY collect from NavMeshOnly
            surface = meshObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.layerMask = LayerMask.GetMask("NavMeshOnly");
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.overrideVoxelSize = true;
            surface.voxelSize = 0.4f; // tune
            surface.overrideTileSize = true;
            surface.tileSize = 128; // tune


            Debug.Log(NavMeshBuildQueue.Instance ? "Queue exists" : "Queue is NULL");
            //NavMeshBuildQueue.Instance.Enqueue(surface, this);
            navMeshLodIndex = detailLevels.Length - 1; // highest LOD index = lowest detail
            prevNMLodIndex = detailLevels.Length;

            

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
            //Debug.Log($"Requesting map data for chunk coord {coord} at world origin {position}");

            Vector2 chunkOrigin = new Vector2(coord.x * size, coord.y * size);
            Vector2 chunkCentre = chunkOrigin + Vector2.one * (size * 0.5f);

            //mapGenerator.RequestMapData(chunkCentre, OnMapDataReceived);
            mapGenerator.RequestMapData(position, size, OnMapDataReceived);
        }
        
        void OnMapDataReceived(MapData mapData){
            // request mesh data
            this.mapData = mapData;
            mapDataReceived = true;

            /*
            VARIOUS DEBUG TEXTURES
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


            try {
                // place dungeons of chunk, default to invisible until highest LOD
                foreach (Vector2Int point in mapData.biomeGenData.dungeonArray){
                    
                    if (point.x < 0 || point.x >= mapData.chunkSize || point.y < 0 || point.y >= mapData.chunkSize){continue;}

                    float worldX = point.x + position.x - 0.5f * mapData.chunkSize;
                    float worldY = position.y - point.y + 0.5f * mapData.chunkSize;

                    // obtain information via chunk-local coords
                    BiomeCoord biomeCoord = mapData.biomeGenData.voronoiMap[point.x, point.y];
                    BiomeScriptableObject biome = mapData.biomeDict[biomeCoord.getBiome()];
                    
                    // position in world space
                    float height = mapData.heightCurve.Evaluate(mapData.noiseMap[point.x, point.y]) * (mapGenerator.meshHeightMultiplier * biome.biomeHeightMultiplier);
                    Vector3 dungeonPos = new Vector3(worldX * scale, height*scale, worldY * scale);
                    
                    GameObject dungeon = GameObject.Instantiate(biome.dungeonPrefab, dungeonPos, Quaternion.identity);
                    dungeon.layer = LayerMask.NameToLayer("Ground");
                    dungeon.transform.parent = meshObject.transform;
                    dungeon.SetActive(false);
                    dungeonList.Add(dungeon);
                }
            }
            catch (Exception e) {
                Debug.LogError($"Dungeon spawn failed for chunk {coord}: {e}");
            }
            // Tree Logic - rememeber to scale height by mult and biome mult
            try {
                // place dungeons of chunk, default to invisible until highest LOD
                foreach (TreeCoord treeCoord in mapData.treeCoords){
                    
                    if (treeCoord.x < 0 || treeCoord.x >= mapData.chunkSize || treeCoord.y < 0 || treeCoord.y >= mapData.chunkSize){continue;}

                    float worldX = treeCoord.x + position.x - 0.5f * mapData.chunkSize;
                    float worldY = position.y + ((0.5f * mapData.chunkSize) - treeCoord.y);

                    // obtain information via chunk-local coords
                    BiomeCoord biomeCoord = mapData.biomeGenData.voronoiMap[treeCoord.x, treeCoord.y];
                    
                    // position in world space
                    float height = mapData.heightCurve.Evaluate(treeCoord.z) * (mapGenerator.meshHeightMultiplier * treeCoord.biomeType.biomeHeightMultiplier);
                    Vector3 treePos = new Vector3(worldX * scale, height*scale, worldY * scale);
                    
                    GameObject tree = GameObject.Instantiate(treeCoord.biomeType.treePrefabs[treeCoord.objectIndex].prefab, treePos, Quaternion.identity);
                    tree.transform.parent = meshObject.transform;
                    tree.layer = LayerMask.NameToLayer("Ground");
                    tree.SetActive(false);
                    treeList.Add(tree);
                }
            }
            catch (Exception e) {
                Debug.LogError($"Tree spawn failed for chunk {coord}: {e}");
            }
            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk(){
            // determine if chunk is visible based on viewer position, visible true if within maxViewDistance
            if (mapDataReceived){
                // Ensure navmesh LOD mesh is requested at least once
                LODmesh navLod = lodMeshes[navMeshLodIndex];
                if (!navLod.hasMesh && !navLod.hasRequestedMesh)
                {
                    navLod.RequestMesh(mapData);
                }
                else
                {
                    navSourceFilter.sharedMesh = navLod.mesh;
                    navSourceCollider.sharedMesh = navLod.mesh;

                    // Only queue nav build once nav source is actually ready
                    if (!navQueued)
                    {
                        navQueued = true;
                        NavMeshBuildQueue.Instance.Enqueue(surface, this);
                    }
                }
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
                            // only place dungeon if highest detail LOD - this will change to visible if close enough to viewer
                            if (lodIndex == 0){
                                foreach (GameObject dungeon in dungeonList){
                                    dungeon.SetActive(true);
                                }
                            }if (lodIndex == 0){
                                // hide trees if not highest LOD
                                foreach (GameObject tree in treeList){
                                    tree.SetActive(true);
                                }
                            }
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                            meshCollider.sharedMesh = lodMesh.mesh;
                            // Update NavMesh source mesh if this is the nav LOD mesh
                            /*if (lodIndex == navMeshLodIndex)
                            {
                                navSourceFilter.sharedMesh = lodMesh.mesh;
                                navSourceCollider.sharedMesh = lodMesh.mesh;

                                // Only queue nav build once nav source is actually ready
                                if (!navQueued)
                                {
                                    navQueued = true;
                                    NavMeshBuildQueue.Instance.Enqueue(surface, this);
                                }
                            }*/



                        } else if (!lodMesh.hasRequestedMesh){
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                    visibleTerrainChunksLastUpdate.Add(this);
                }
                else{
                    // hide dungeons  and treesif chunk not visible
                    foreach (GameObject dungeon in dungeonList){
                        dungeon.SetActive(false);
                    }
                    foreach (GameObject tree in treeList){
                        tree.SetActive(false);
                    }
                }
                SetVisible(visible);

            }
        }

        public void SetVisible(bool visible){
            // sets object to visible or not
            if (meshObject != null) meshObject.SetActive(visible);
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
