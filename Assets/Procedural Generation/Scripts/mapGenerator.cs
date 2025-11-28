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
    
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    [SerializeField] public Color[] cellColours;
    [SerializeField] public int numOfCells = 10;

    public bool autoUpdate;

    public TerrainType[] regions;

    //public Biomes[] biomes;
    public BiomeScriptableObject[] Biomes;

    public void generateMap(){
        // call noise.GenerateNoiseMap() with parameters to generate noise map
        float[,] noiseMap = noise.GenerateNoiseMap (mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        // build a 1d array of colours by looping through the heightmap, checking TerrainType struct, and assigning colours accordingly EXPAND WITH BIOMES - i.e. figure out different structs for different biomes
        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y=0; y < mapHeight; y++){
            for (int x=0; x < mapWidth; x++){

                float currentHeight = noiseMap[x,y];
                for (int i = 0; i < regions.Length; i++){
                    if (currentHeight <= regions[i].height){
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }


        // find displayMap object, and draw noisemap
        mapDisplay display = Object.FindFirstObjectByType<mapDisplay> ();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture (TextureGenerator.TextureFromHeightMap(noiseMap));
        }else if (drawMode == DrawMode.ColourMap){
            display.DrawTexture (TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }else if (drawMode == DrawMode.Mesh){
            display.DrawMesh (meshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }else if (drawMode == DrawMode.Voronoi){
            // Iterate through biomes list to obtain voronoi colours
            Color[] voronoiColours = new Color[Biomes.Length];
            for (int i=0; i < Biomes.Length; i++){
                voronoiColours[i] = Biomes[i].biomeColour;
                Debug.Log("Added colour: " + Biomes[i].biomeColour);
            }
            display.DrawTexture (TextureGenerator.TextureFromColourMap(VoronoiGenerator.GenerateVDiagram(mapWidth, mapHeight, voronoiColours, numOfCells, seed), mapWidth, mapHeight));
        }

    }

    void onValidate (){
        if (mapWidth < 1) {
            mapWidth = 1;
        }
        if (mapHeight < 1){
            mapHeight = 1;
        }
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
