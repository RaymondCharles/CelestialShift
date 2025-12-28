using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class noise
{
    public static float[,] GenerateNoiseMap (int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, BiomeGenData biomeGenData, BiomeScriptableObject[] Biomes){
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

        // Integrate Voronoi
        //string [,] VoronoiMap = VoronoiGenerator.GenerateVDiagram(mapWidth, mapHeight, new Color[] {Color.black, Color.white}, 10, seed);
        // struct in mapdisplay for biome variables, pass in as parameter - then no need for so many variables can just be one struct
        // within perlin loop, check which biome it is in, use those variables
        // new loop checking weights then can use that to multiply variables by a factor of each - use curve too, maybe in editor for how close to border to blend - then pass that into VD generate method, for each one, after finding closest, try this Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]); using min as next closest biome, max as this biome, value is distance between the two, then check curve to see if you should apply it or not

        foreach (Vector2Int coord in biomeGenData.buildingPointsArray){
            noiseMap[coord.x, coord.y] = 0.5f; // flatten building points
            noiseMap[coord.x-1, coord.y-1] = 0.5f;
            noiseMap[coord.x+1, coord.y+1] = 0.5f;
            noiseMap[coord.x+1, coord.y-1] = 0.5f;
            noiseMap[coord.x-1, coord.y+1] = 0.5f;
            noiseMap[coord.x, coord.y-1] = 0.5f;
            noiseMap[coord.x, coord.y+1] = 0.5f;
            noiseMap[coord.x-1, coord.y] = 0.5f;
            noiseMap[coord.x+1, coord.y] = 0.5f;
        }

        // normalizes values between 0 and 1 using max and minimum noiseheight
        for (int y=0; y < mapHeight; y++){
            for (int x=0; x < mapWidth; x++){
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
            }
        }
        return noiseMap;
    }
}
