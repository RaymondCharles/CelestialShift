using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator{

    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height){
        Texture2D texture = new Texture2D (width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels (colourMap);
        texture.Apply ();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap){
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        // 1d array to store colours for noisemap in order to get a visual representation
        Color [] colourMap = new Color[width * height];
        for (int y=0; y < height; y++){
            for (int x=0; x < height; x++){
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap [x, y]);
            }
        }
        return TextureFromColourMap(colourMap, width, height);
    }

    public static Texture2D TextureFromBiomeMap(BiomeGenData biomeGenData, BiomeScriptableObject[] biomes){
        int width = biomeGenData.voronoiMap.GetLength(0);
        int height = biomeGenData.voronoiMap.GetLength(1);

        // find more efficient way: enums or a class or dict or smt in mapgen
        Dictionary<string, Color> biomeColourDict = new Dictionary<string, Color>();
        for (int i = 0; i < biomes.Length; i++){
            biomeColourDict.Add(biomes[i].name , biomes[i].biomeColour);
        }

        // 1d array to store colours for biomemap in order to get a visual representation
        Color [] colourMap = new Color[width * height];
        for (int y=0; y < height; y++){
            for (int x=0; x < height; x++){
                Color c = Color.Lerp(biomeColourDict[biomeGenData.voronoiMap[x,y].getBiome()], biomeColourDict[biomeGenData.voronoiMap[x,y].getSecondBiome()], biomeGenData.voronoiMap[x,y].getSecondWeight());
                colourMap[y * width + x] = Color.Lerp(c, biomeColourDict[biomeGenData.voronoiMap[x,y].getThirdBiome()], biomeGenData.voronoiMap[x,y].getThirdWeight());
            }
        }
        // inefficient but works for now
        for (int i = 0; i < biomeGenData.buildingPointsArray.GetLength(0); i++){
            for (int j = 0; j < biomeGenData.buildingPointsArray.GetLength(1); j++){
                Vector2Int point = biomeGenData.buildingPointsArray[i,j];
                int x = (int)point.x;
                int y = (int)point.y;
                colourMap[y * width + x] = Color.black;
            }
        }
        return TextureFromColourMap(colourMap, width, height);
    }
}