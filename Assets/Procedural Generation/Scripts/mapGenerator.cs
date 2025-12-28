using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public TerrainType[] regions;

    //public Biomes[] biomes;
    // efficiency improvement: use dict?
    public BiomeScriptableObject[] Biomes;

    public int numOfCells;

    [Range(0, 240)]

    public float blendWidth = 20f;
    
    
    public void DrawMapInEditor(){
        MapData mapData = generateMapData();
        
        Color[] voronoiColours = new Color[Biomes.Length];
        for (int i=0; i < Biomes.Length; i++){
                voronoiColours[i] = Biomes[i].biomeColour;
                Debug.Log("Added colour: " + Biomes[i].biomeColour);
            }

        // find displayMap object, and draw noisemap
        mapDisplay display = Object.FindFirstObjectByType<mapDisplay> ();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture (TextureGenerator.TextureFromHeightMap(mapData.noiseMap));
        }else if (drawMode == DrawMode.ColourMap){
            display.DrawTexture (TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }else if (drawMode == DrawMode.Mesh){
            display.DrawMesh (meshGenerator.GenerateTerrainMesh(mapData.noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }else if (drawMode == DrawMode.Voronoi){
            display.DrawTexture (TextureGenerator.TextureFromBiomeMap(mapData.biomeGenData, Biomes));
        }
    }

    MapData generateMapData(){
        Color[] voronoiColours = new Color[Biomes.Length];
        for (int i=0; i < Biomes.Length; i++){
                voronoiColours[i] = Biomes[i].biomeColour;
                Debug.Log("Added colour: " + Biomes[i].biomeColour);
            }

        //BiomeCoord[,] voronoiMap = VoronoiGenerator.GenerateVDiagram(mapChunkSize, mapChunkSize, voronoiColours, numOfCells, seed, Biomes, blendWidth);
        BiomeGenData biomeGenData = VoronoiGenerator.GenerateVDiagram(mapChunkSize, mapChunkSize, voronoiColours, numOfCells, seed, Biomes, blendWidth);

        
        // call noise.GenerateNoiseMap() with parameters to generate noise map
        float[,] noiseMap = noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset, biomeGenData.voronoiMap, Biomes);

        // build a 1d array of colours by looping through the heightmap, checking TerrainType struct, and assigning colours accordingly EXPAND WITH BIOMES - i.e. figure out different structs for different biomes
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y=0; y < mapChunkSize; y++){
            for (int x=0; x < mapChunkSize; x++){
                float currentHeight = noiseMap[x,y];
                string currentBiome = biomeGenData.voronoiMap[x,y].getBiome();
                TerrainType[] biomeRegions = Biomes[0].terrainType; // defaults to first biome
                // OPTIMIZE: change to dict or something AND LOOP IS BADDDDD
                foreach (BiomeScriptableObject biome in Biomes) { if (biome.name == currentBiome) {biomeRegions = biome.terrainType;}};
                for (int i = 0; i < biomeRegions.Length; i++){
                    if (currentHeight <= biomeRegions[i].height){
                        colourMap[y * mapChunkSize + x] = biomeRegions[i].colour;
                        break;
                    }
                }
            }
        }

        return new MapData (noiseMap, colourMap, biomeGenData);
    }

    void onValidate (){
        if (lacunarity < 1){
            lacunarity = 1;
        }
        if (octaves < 0){
            octaves = 0;
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
    public float[,] noiseMap;
    public Color[] colourMap;
    public BiomeGenData biomeGenData;


    public MapData(float[,] noiseMap, Color[] colourMap, BiomeGenData biomeGenData){
        this.noiseMap = noiseMap;
        this.colourMap = colourMap;
        this.biomeGenData = biomeGenData;
    }
}
