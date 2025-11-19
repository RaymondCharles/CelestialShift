using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class noise
{
    public static float[,] GenerateNoiseMap (int mapWidth, int mapHeight, float scale){
        // Create 2d float array, iterate through it and assign noise values
        float [,] noiseMap = new float[mapWidth, mapHeight];
        if (scale <= 0) {
            scale = 0.0001f;
        }
        for (int y=0; y < mapHeight; y++){
            for (int x=0; y < mapWidth; x++){
                // Cast int x and y to float, divide by scale to add variety to values
                float sampleX = x / scale;
                float sampleY = y / scale;

                // Set 2d array coordinate to perlin noised value
                float perlinValue = Mathf.PerlinNoise (sampleX, sampleY);
                noiseMap [x,y] = perlinValue;
            }
        }
        return noiseMap;
    }
}
