using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class mapGenerator : MonoBehaviour
{
    // enum to select which 
    public enum DrawMode{
        NoiseMap, ColourMap, Mesh, Voronoi
    }
    public DrawMode drawMode;
    
    public const int mapChunkSize = 241;
    [Range(0,6)] // clamped to 0-6 to prevent LOD errors
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;
    public bool generateBuildings;

    public TerrainType[] regions;

    //public Biomes[] biomes;
    // efficiency improvement: use dict?
    public BiomeScriptableObject[] Biomes;
    public MapDataScriptableObject mapDataScriptableObject;
    //public Dictionary<string, BiomeScriptableObject> biomeDict = new Dictionary<string, BiomeScriptableObject>();

    public GameObject mesh; // parent object to hold spawned buildings

    public int numOfCells;

    [Range(0, 240)]

    public float blendWidth = 20f;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void GenerateMapDataInEditor(){
        mapDataScriptableObject.mapData = generateMapData();
    }
    
    public void DrawMapInEditor(){
        Color[] voronoiColours = new Color[Biomes.Length];
        for (int i=0; i < Biomes.Length; i++){
                voronoiColours[i] = Biomes[i].biomeColour;
                //Debug.Log("Added colour: " + Biomes[i].biomeColour);
            }

        // find displayMap object, and draw noisemap
        mapDisplay display = UnityEngine.Object.FindFirstObjectByType<mapDisplay> ();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture (TextureGenerator.TextureFromHeightMap(mapDataScriptableObject.mapData.noiseMap));
        }else if (drawMode == DrawMode.ColourMap){
            display.DrawTexture (TextureGenerator.TextureFromColourMap(mapDataScriptableObject.mapData.colourMap, mapChunkSize, mapChunkSize));
        }else if (drawMode == DrawMode.Mesh){
            // draw mesh, spawn buildings at building points
            display.DrawMesh (meshGenerator.GenerateTerrainMesh(mapDataScriptableObject.mapData.noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapDataScriptableObject.mapData.colourMap, mapChunkSize, mapChunkSize));
            if (generateBuildings){
                foreach (Vector2Int coord in mapDataScriptableObject.mapData.biomeGenData.buildingPointsArray){
                    if (!mapDataScriptableObject.mapData.buildingsMap[coord.x, coord.y]){ // only spawn if not already spawned
                        Debug.Log("Spawning building at: " + coord);
                        spawnObject.SpawnObjectAtPoint(mapDataScriptableObject.mapData.biomeDict[mapDataScriptableObject.mapData.biomeGenData.voronoiMap[coord.x, coord.y].getBiome()].buildingPrefab, new Vector3(coord.x, 0.5f * meshHeightMultiplier, coord.y), Quaternion.identity, mesh.transform);
                        mapDataScriptableObject.mapData.buildingsMap[coord.x, coord.y] = true; // mark as spawned
                    }
                }
            }
        }else if (drawMode == DrawMode.Voronoi){
            display.DrawTexture (TextureGenerator.TextureFromBiomeMap(mapDataScriptableObject.mapData.biomeGenData, Biomes));
        }
    }

    public void RequestMapData(Action<MapData> callback){
        // start mapGeneration on new thread
        ThreadStart threadStart = delegate {
            MapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callback){
        MapData mapData = generateMapData();
        lock (mapDataThreadInfoQueue){
            // enqueue the generated map data to be processed on main thread, locked so no conflicts
            mapDataThreadInfoQueue.Enqueue (new MapThreadInfo<MapData> (callback, mapData));
        }
    }

    public void RequestMeshData(MapData  mapData,Action<MeshData> callback){
        ThreadStart threadStart = delegate {
            MeshDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(Action<MeshData> callback){
        MeshData meshData = meshGenerator.GenerateTerrainMesh(mapDataScriptableObject.mapData.noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
        lock (meshDataThreadInfoQueue){
            // enqueue the generated mesh data to be processed on main thread, locked so no conflicts
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update(){
        // process any map data that has been generated on other threads
        if (mapDataThreadInfoQueue.Count > 0){
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++){
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0){
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++){
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }
    
    public MapData generateMapData(){
        // create biome dictionary for easy access
        Dictionary<string, BiomeScriptableObject> biomeDict = new Dictionary<string, BiomeScriptableObject>();
        bool[,] buildingsMap = new bool[mapChunkSize, mapChunkSize]; // OPTIMIZE WE DO NOT NEED A FULL MAP HERE
        foreach (BiomeScriptableObject biome in Biomes) {
            biomeDict[biome.name] = biome;
        }

        Color[] voronoiColours = new Color[Biomes.Length];
        for (int i=0; i < Biomes.Length; i++){
                voronoiColours[i] = Biomes[i].biomeColour;
                //Debug.Log("Added colour: " + Biomes[i].biomeColour);
            }

        //BiomeCoord[,] voronoiMap = VoronoiGenerator.GenerateVDiagram(mapChunkSize, mapChunkSize, voronoiColours, numOfCells, seed, Biomes, blendWidth);
        BiomeGenData biomeGenData = VoronoiGenerator.GenerateVDiagram(mapChunkSize, mapChunkSize, voronoiColours, numOfCells, seed, Biomes, blendWidth);

        
        // call noise.GenerateNoiseMap() with parameters to generate noise map
        float[,] noiseMap = noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset, biomeGenData, Biomes, biomeDict);

        // build a 1d array of colours by looping through the heightmap, checking TerrainType struct, and assigning colours accordingly EXPAND WITH BIOMES - i.e. figure out different structs for different biomes
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y=0; y < mapChunkSize; y++){
            for (int x=0; x < mapChunkSize; x++){
                float currentHeight = noiseMap[x,y];
                string currentBiome = biomeGenData.voronoiMap[x,y].getBiome();

                TerrainType[] firstBiomeRegions = biomeDict[currentBiome].terrainType;
                TerrainType[] secondBiomeRegions = biomeDict[biomeGenData.voronoiMap[x,y].getSecondBiome()].terrainType;
                TerrainType[] thirdBiomeRegions = biomeDict[biomeGenData.voronoiMap[x,y].getThirdBiome()].terrainType;

                int f = 0;
                int s = 0;
                int t = 0;

                while (currentHeight > firstBiomeRegions[f].height || currentHeight > secondBiomeRegions[s].height || currentHeight > thirdBiomeRegions[t].height) {
                    if (currentHeight > firstBiomeRegions[f].height) {f++;}
                    if (currentHeight > secondBiomeRegions[s].height) {s++;}
                    if (currentHeight > thirdBiomeRegions[t].height) {t++;}     
                }

                Color firstColour = firstBiomeRegions[f].colour;
                Color secondColour = secondBiomeRegions[s].colour;
                Color thirdColour = thirdBiomeRegions[t].colour;

                Color c = Color.Lerp(firstColour, secondColour, biomeGenData.voronoiMap[x,y].getSecondWeight());
                colourMap[y * mapChunkSize + x] = Color.Lerp(c, thirdColour, biomeGenData.voronoiMap[x,y].getThirdWeight());

                /*
                // OPTIMIZE: change to dict or something AND LOOP IS BADDDDD
                foreach (BiomeScriptableObject biome in Biomes) { if (biome.name == currentBiome) {biomeRegions = biome.terrainType;}};
                for (int i = 0; i < biomeRegions.Length; i++){
                    if (currentHeight <= biomeRegions[i].height){
                        colourMap[y * mapChunkSize + x] = biomeRegions[i].colour;
                        break;
                    }
                }
                */
            }
        }

        // set a 9 pixel square at building points to red to visualize them
            Color redColor = new Color(1f, 0f, 0f);
        if (generateBuildings){
            foreach (Vector2Int coord in biomeGenData.buildingPointsArray){
                if (coord.x > 2 && coord.x < mapChunkSize -2 && coord.y > 2 && coord.y < mapChunkSize -2){
                    colourMap[coord.y * mapChunkSize + coord.x] = redColor; // colour building points
                    colourMap[(coord.y-1) * mapChunkSize + (coord.x-1)] = redColor;
                    colourMap[(coord.y+1) * mapChunkSize + (coord.x+1)] = redColor;
                    colourMap[(coord.y+1) * mapChunkSize + (coord.x-1)] = redColor;
                    colourMap[(coord.y-1) * mapChunkSize + (coord.x+1)] = redColor;
                    colourMap[(coord.y) * mapChunkSize + (coord.x-1)] = redColor;
                    colourMap[(coord.y) * mapChunkSize + (coord.x+1)] = redColor;  
                    colourMap[(coord.y-1) * mapChunkSize + (coord.x)] = redColor;
                    colourMap[(coord.y+1) * mapChunkSize + (coord.x)] = redColor;
                }
                buildingsMap[coord.x, coord.y] = false; // mark building point as unspawned
                Debug.Log("Marked building at: " + coord + " in buildings map." + ": " + buildingsMap[coord.x, coord.y] );
            }
        }
        return new MapData (noiseMap, colourMap, biomeGenData, biomeDict, buildingsMap);
    }

    void onValidate (){
        if (lacunarity < 1){
            lacunarity = 1;
        }
        if (octaves < 0){
            octaves = 0;
        }
    }

    struct MapThreadInfo<T>{
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter){
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}


// add new struct with terraintype as a field, biomes, adding lacunarity and all that good stuff as fields
[System.Serializable]
public struct TerrainType{
    public string name;
    public float height;
    public Color colour;
}

[System.Serializable]
public struct Biomes{
    public string name;
    public TerrainType[] terrainType;
    public float noiseScale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public Color biomeColour;
}

public struct MapData{
    public readonly float[,] noiseMap;
    public readonly Color[] colourMap;
    public readonly BiomeGenData biomeGenData;
    public readonly Dictionary<string, BiomeScriptableObject> biomeDict;
    public readonly bool[,] buildingsMap;

    public MapData(float[,] noiseMap, Color[] colourMap, BiomeGenData biomeGenData, Dictionary<string, BiomeScriptableObject> biomeDict, bool[,] buildingsMap){
        this.noiseMap = noiseMap;
        this.colourMap = colourMap;
        this.biomeGenData = biomeGenData;
        this.biomeDict = biomeDict;
        this.buildingsMap = buildingsMap;
    }
}
