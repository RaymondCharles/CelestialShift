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
    public BiomeScriptableObject[] Biomes;

    public int numOfCells;

    private string[,] VoronoiMap;
    private Color[] voronoiColours;
    
    public void DrawMapInEditor(){

        voronoiColours = new Color[Biomes.Length];
        for (int i=0; i < Biomes.Length; i++){
                voronoiColours[i] = Biomes[i].biomeColour;
                Debug.Log("Added colour: " + Biomes[i].biomeColour);
            }
        VoronoiMap = VoronoiGenerator.GenerateVDiagram(mapChunkSize, mapChunkSize, voronoiColours, numOfCells, seed, Biomes);

        // find displayMap object, and draw noisemap
        mapDisplay display = Object.FindFirstObjectByType<mapDisplay> ();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture (TextureGenerator.TextureFromHeightMap(noiseMap));
        }else if (drawMode == DrawMode.ColourMap){
            display.DrawTexture (TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }else if (drawMode == DrawMode.Mesh){
            display.DrawMesh (meshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }else if (drawMode == DrawMode.Voronoi){
            // Iterate through biomes list to obtain voronoi colours#
            // find more efficient way: enums or a class or dict or smt in mapgen
            Color[] voronoiColours = new Color[Biomes.Length];
            for (int i=0; i < Biomes.Length; i++){
                voronoiColours[i] = Biomes[i].biomeColour;
                Debug.Log("Added colour: " + Biomes[i].biomeColour);
            }
            display.DrawTexture (TextureGenerator.TextureFromBiomeMap(VoronoiMap, Biomes));
        }
    }

    public void generateMap(){
        voronoiColours = new Color[Biomes.Length];
        for (int i=0; i < Biomes.Length; i++){
                voronoiColours[i] = Biomes[i].biomeColour;
                Debug.Log("Added colour: " + Biomes[i].biomeColour);
            }
        VoronoiMap = VoronoiGenerator.GenerateVDiagram(mapChunkSize, mapChunkSize, voronoiColours, numOfCells, seed, Biomes);

        
        // call noise.GenerateNoiseMap() with parameters to generate noise map
        float[,] noiseMap = noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset, VoronoiMap, Biomes);

        // build a 1d array of colours by looping through the heightmap, checking TerrainType struct, and assigning colours accordingly EXPAND WITH BIOMES - i.e. figure out different structs for different biomes
        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y=0; y < mapChunkSize; y++){
            for (int x=0; x < mapChunkSize; x++){

                float currentHeight = noiseMap[x,y];
                for (int i = 0; i < regions.Length; i++){
                    if (currentHeight <= regions[i].height){
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

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
    public float[,] heightMap;
    public Color[] colourMap;
}
