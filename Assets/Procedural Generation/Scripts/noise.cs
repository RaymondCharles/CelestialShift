using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class noise
{
    public static float[,] GenerateNoiseMap (int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset){
        // Create 2d float array, iterate through it and assign noise values
        float [,] noiseMap = new float[mapWidth, mapHeight];
        
        // Get random offset values for each octave
        System.Random prng = new System.Random (seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i=0; i < octaves; i++){
            float offsetX = prng.Next (-100000, 100000) + offset.x;
            float offsetY = prng.Next (-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2 (offsetX, offsetY);
        }

        if (scale <= 0) {
            scale = 0.0001f;
        }

        // keep track of max and min noise height in order to normalise noise map back to range 0-1 at end
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y=0; y < mapHeight; y++){
            for (int x=0; x < mapWidth; x++){
                
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i=0; i < octaves; i++){
                    // Cast int x and y to float, divide by scale to add variety to values
                    float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;

                    // Set 2d array coordinate to perlin noised value
                    float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight){
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight){
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x,y] = noiseHeight;
            }
        }

        for (int y=0; y < mapHeight; y++){
            for (int x=0; x < mapWidth; x++){
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
            }
        }
        return noiseMap;
    }
}
