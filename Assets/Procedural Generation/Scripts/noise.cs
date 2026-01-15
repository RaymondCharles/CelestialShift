using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class noise
{
    // OPTIMIZATION: can we use a struct for variables?

    public enum NormalizeMode {Local, Global};
    public static float[,] GenerateNoiseMap (int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, BiomeGenData biomeGenData, BiomeScriptableObject[] Biomes, Dictionary<string, BiomeScriptableObject> biomeDict, NormalizeMode normalizeMode){
        // Create 2d float array, iterate through it and assign noise values
        float [,] noiseMap = new float[mapWidth, mapHeight];
        
        // Get random offset values for each octave
        System.Random prng = new System.Random (seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;
        foreach (BiomeScriptableObject biome in Biomes){
            persistance = Mathf.Max(biome.persistance, persistance);
        }

        for (int i=0; i < octaves; i++){
            float offsetX = prng.Next (-100000, 100000) + offset.x;
            float offsetY = prng.Next (-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2 (offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0) {
            scale = 0.0001f;
        }

        // keep track of max and min noise height in order to normalise noise map back to range 0-1 at end
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        var biomeMaxHeight = new Dictionary<string, float>(biomeDict.Count);
        foreach (var kv in biomeDict)
        {
            float p = Mathf.Clamp01(kv.Value.persistance);
            float amp = 1f;
            float sum = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += amp;
                amp *= p;
            }
            biomeMaxHeight[kv.Key] = Mathf.Max(0.0001f, sum);
        }

        for (int y=0; y < mapHeight; y++){
            for (int x=0; x < mapWidth; x++){
                // get biomes for this coordinate
                string firstBiome = biomeGenData.voronoiMap[x,y].getBiome();
                string secondBiome = biomeGenData.voronoiMap[x,y].getSecondBiome();
                string thirdBiome = biomeGenData.voronoiMap[x,y].getThirdBiome();
                //Debug.Log("First Biome: " + firstBiome + " Second Biome: " + secondBiome + " Third Biome: " + thirdBiome);
                
                // set initial amplitude and frequency values for all biomes
                amplitude = 1;
                float secondAmplitude = 1;
                float thirdAmplitude = 1;

                frequency = 1;
                float secondFrequency = 1;
                float thirdFrequency = 1;

                float noiseHeight = 0;
                float secondnoiseHeight = 0;
                float thirdNoiseHeight = 0;
                float finalNoiseHeight = 0;

                // set biome specific variables
                float biomePersistance = biomeDict[firstBiome].persistance;
                float biomeLacunarity = biomeDict[firstBiome].lacunarity;
                //float biomemeshHeightMultiplier = biomeDict[firstBiome].meshHeightMultiplier;

                float secondBiomePersistance = biomeDict[secondBiome].persistance;
                float secondBiomeLacunarity = biomeDict[secondBiome].lacunarity;
                //float secondBiomemeshHeightMultiplier = biomeDict[secondBiome].meshHeightMultiplier;

                float thirdBiomePersistance = biomeDict[thirdBiome].persistance;
                float thirdBiomeLacunarity = biomeDict[thirdBiome].lacunarity;
                //float thirdBiomemeshHeightMultiplier = biomeDict[thirdBiome].meshHeightMultiplier;
                for (int i=0; i < octaves; i++){
                    // Cast int x and y to float, divide by scale to add variety to values
                    float sampleX = (x-halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y-halfHeight + octaveOffsets[i].y) / scale * frequency;
                    float sampleX1 = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;

                    float sampleX2 = (x - halfWidth + octaveOffsets[i].x) / scale * secondFrequency;
                    float sampleY2 = (y - halfHeight + octaveOffsets[i].y) / scale * secondFrequency;

                    float sampleX3 = (x - halfWidth + octaveOffsets[i].x) / scale * thirdFrequency;
                    float sampleY3 = (y - halfHeight + octaveOffsets[i].y) / scale * thirdFrequency;

                    // Set 2d array coordinate to perlin noised value
                    float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                    float perlin2 = Mathf.PerlinNoise(sampleX2, sampleY2) * 2 - 1;
                    float perlin3 = Mathf.PerlinNoise(sampleX3, sampleY3) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;
                    secondnoiseHeight += perlin2 * secondAmplitude;
                    thirdNoiseHeight += perlin3 * thirdAmplitude;
                    
                    // adjust amplitude and frequency for next octave for each biome
                    amplitude *= biomePersistance;
                    frequency *= biomeLacunarity;
                    secondAmplitude *= secondBiomePersistance;
                    secondFrequency *= secondBiomeLacunarity;
                    thirdAmplitude *= thirdBiomePersistance;
                    thirdFrequency *= thirdBiomeLacunarity;
                }

                /*
                // multiply all 3 biome noise height values by their respective mesh height multipliers, then blend based on voronoi weights
                noiseHeight = (noiseHeight + 1) / 2f * biomemeshHeightMultiplier;
                secondnoiseHeight = (secondnoiseHeight + 1) / 2f * secondBiomemeshHeightMultiplier;
                thirdNoiseHeight = (thirdNoiseHeight + 1) / 2f * thirdBiomemeshHeightMultiplier;

                
                finalNoiseHeight = Mathf.Lerp(noiseHeight, secondnoiseHeight, biomeGenData.voronoiMap[x,y].getSecondWeight());;
                finalNoiseHeight = Mathf.Lerp(finalNoiseHeight, thirdNoiseHeight, biomeGenData.voronoiMap[x,y].getThirdWeight());
                */

                float h1 = (noiseHeight + biomeMaxHeight[firstBiome]) / (2f * biomeMaxHeight[firstBiome]);
                float h2 = (secondnoiseHeight + biomeMaxHeight[secondBiome]) / (2f * biomeMaxHeight[secondBiome]);
                float h3 = (thirdNoiseHeight + biomeMaxHeight[thirdBiome]) / (2f * biomeMaxHeight[thirdBiome]);

                h1 = Mathf.Clamp01(h1);
                h2 = Mathf.Clamp01(h2);
                h3 = Mathf.Clamp01(h3);

                finalNoiseHeight = Mathf.Lerp(h1, h2, biomeGenData.voronoiMap[x,y].getSecondWeight());
                finalNoiseHeight = Mathf.Lerp(finalNoiseHeight, h3, biomeGenData.voronoiMap[x,y].getThirdWeight());

                if (finalNoiseHeight > maxLocalNoiseHeight){
                    maxLocalNoiseHeight = finalNoiseHeight;
                } else if (finalNoiseHeight < minLocalNoiseHeight){
                    minLocalNoiseHeight = finalNoiseHeight;
                }
                noiseMap[x,y] = finalNoiseHeight;
            }
        }

        for (int y=0; y < mapHeight; y++){
            for (int x=0; x < mapWidth; x++){
                if (normalizeMode == NormalizeMode.Local){
                    noiseMap[x,y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x,y]);
                }else{
                }
            }
        }

        // Integrate Voronoi
        //string [,] VoronoiMap = VoronoiGenerator.GenerateVDiagram(mapWidth, mapHeight, new Color[] {Color.black, Color.white}, 10, seed);
        // struct in mapdisplay for biome variables, pass in as parameter - then no need for so many variables can just be one struct
        // within perlin loop, check which biome it is in, use those variables, integrating weight of each biome
        // new loop checking weights then can use that to multiply variables by a factor of each - use curve too, maybe in editor for how close to border to blend - then pass that into VD generate method, for each one, after finding closest, try this Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]); using min as next closest biome, max as this biome, value is distance between the two, then check curve to see if you should apply it or not

        /*
        foreach (Vector2Int coord in biomeGenData.buildingPointsArray){
            // will need to blend surrouding area, get size of building from scriptable object
            noiseMap[coord.x, coord.y] = 0.5f; // flatten building points
            if (coord.x > 0 && coord.y > 0 && coord.x < mapWidth -1 && coord.y < mapHeight -1){
                noiseMap[coord.x-1, coord.y] = 0.5f;
                noiseMap[coord.x-1, coord.y-1] = 0.5f;
                noiseMap[coord.x-1, coord.y+1] = 0.5f;
                noiseMap[coord.x+1, coord.y-1] = 0.5f;
                noiseMap[coord.x+1, coord.y+1] = 0.5f;
                noiseMap[coord.x, coord.y-1] = 0.5f;
                noiseMap[coord.x, coord.y+1] = 0.5f;
                noiseMap[coord.x+1, coord.y] = 0.5f;
            }
        } */
        return noiseMap;
    }
}
 