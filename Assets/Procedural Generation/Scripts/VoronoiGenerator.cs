using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO
// 1. Add Noise to edges
// 2. Change to make voronoi generic, i.e. return VoronoiMap of floats, then move colour logic to perlin one
// 3. Optimise points generation code - generate in main method or see if needed for other return
// 4. Maybe tie noiseScale to no of cells
// 5. Add biome warpFreq and warpStrength parameters, blendWidth parameter for biome blending
public static class VoronoiGenerator{
    public static BiomeGenData GenerateVDiagram(int width, int height, Color[] cellColours, int numOfCells, int seed, BiomeScriptableObject[] biomes, float blendWidth)
    {
        // Create ColourMap, loop through pixels, assign colour according to Voronoi logic
        // Ensure we have at least one cell and at least one pixel per cell
        int cells = Mathf.Max(1, numOfCells);
        int pixelsPerCell = Mathf.Max(1, width / cells);

        System.Random prng = new System.Random (seed);

        // Create Colour Map
        //Color[] colourMap = new Color[width * height];

        // Create Maps to be returned
        BiomeCoord[,] biomeMap = new BiomeCoord [width , height];
        Vector2Int[,] buildingPointsArray = new Vector2Int[numOfCells, numOfCells];

        // Generate Points and CellColours array
        Vector2Int[,] pointsPosArray = GeneratePoints(numOfCells, pixelsPerCell, prng, 1); // Array to hold cell point positions
        string [,] pointBiomeMap = GeneratePointBiomes(numOfCells, pixelsPerCell, biomes, prng);// Dictionary to hold biome assignment for each point
        
        float warpFreq = (float)numOfCells / (float)width;
        float warpStrength = 15f;

        /*** test code to visualize points only
        for (int x = 0; x < imgSize; x++)
        {
            for (int y = 0; y < imgSize; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }

        for (int i = 0; i < numOfCells; i++)
        {
            for (int j = 0; j < numOfCells; j++)
            {
                texture.SetPixel(pointsPosArray[i, j].x, pointsPosArray[i, j].y, Color.black);
            }
        }
        ***/

        float closestDist;
        float secondClosestDist;
        float thirdClosestDist;
        Vector2Int closestCell;
        Vector2Int secondClosestCell;
        Vector2Int thirdClosestCell;
        // Loop through each pixel to determine its closest point, and assign color accordingly
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get the grid position of the current pixel
                int gridX = x / pixelsPerCell;
                int gridY = y / pixelsPerCell;

                // Reset closest distance and cell for each pixel
                closestDist = Mathf.Infinity;
                secondClosestDist = Mathf.Infinity;
                thirdClosestDist = Mathf.Infinity;
                closestCell = new Vector2Int();
                secondClosestCell = new Vector2Int();
                thirdClosestCell = new Vector2Int();

                for (int i = -1; i < 2; i++)
                // 
                {
                    for (int j = -1; j < 2; j++)
                    {
                        
                        // Calculate the pixel coordinates
                        int X = gridX + i;
                        int Y = gridY + j;
                        // add noise before biome assignment to avoid straight lines
                        float nx = Mathf.PerlinNoise(x * warpFreq, y * warpFreq) - 0.5f;
                        float ny = Mathf.PerlinNoise((x + 1000) * warpFreq, (y + 1000) * warpFreq) - 0.5f;
                        
                        Vector2 warpedSample = new Vector2(
                            x + nx * warpStrength,
                            y + ny * warpStrength
                        );

                        if (X < 0 || X >= numOfCells || Y < 0 || Y >= numOfCells)
                        {
                            continue;
                        }

                        // Create Vector for distance calculation
                        float distance = Vector2.Distance(warpedSample, new Vector2(pointsPosArray[X, Y].x, pointsPosArray[X, Y].y));

                        // Once loop exits, we have the closest 2 cells
                        if (distance < closestDist)
                        {
                            secondClosestDist = closestDist;
                            closestDist = distance;

                            secondClosestCell = closestCell;
                            closestCell = new Vector2Int(X, Y);
                        }
                        else if (distance < secondClosestDist)
                        {
                            secondClosestDist = distance;
                            secondClosestCell = new Vector2Int(X, Y);
                        }
                    }
                }
                // Once looped through all nearby points, assign color of closest cell
                //colourMap[y * width + x] = cellColorsArray[closestCell.x, closestCell.y];
                //biomeMap[x, y] = pointBiomeMap[closestCell.x, closestCell.y];

                // Calculate weights based on distances to border between closest and second closest cells
                /*
                float weight = 1f;
                float secondWeight = 0f;
                float edgeDist = secondClosestDist - closestDist;

                // If close to a border, blend between biomes based on distance
                if (edgeDist < blendWidth)
                {
                    float totalDist = closestDist + secondClosestDist;
                    weight = 1 - (closestDist / totalDist);
                    secondWeight = secondClosestDist / totalDist;
                }
                */
                
                float weight = 1f;
                float secondWeight = 0f;
                float thirdWeight = 0f;
                //float edgeDist = thirdClosestDist - secondClosestDist - closestDist;

                /*
                // If close to a border, blend between biomes based on distance
                if (edgeDist < blendWidth)
                {
                    //float t = Mathf.Clamp(1 - (edgeDist / blendWidth), 0, 0.5f);
                    float sum = closestDist + secondClosestDist + thirdClosestDist;
                    float t = closestDist / sum;
                    t = t * t * (3f - (2f * t)); // Smoothstep
                    weight = 1f - t;
                    float sum2 = secondClosestDist + thirdClosestDist;
                    float t2 = secondClosestDist / sum2;
                    secondWeight = t2;
                    thirdWeight = 1f - t2;
                }
                */
                // inverse distances to get weights (closer = higher weight)
                float w1 = 1f / (closestDist + 0.0001f);
                float w2 = 1f / (secondClosestDist + 0.0001f);
                float w3 = 1f / (thirdClosestDist + 0.0001f);

                // determine if near edge or corner
                float edgeDist = secondClosestDist - closestDist;
                bool nearEdge   = edgeDist < blendWidth;
                bool nearCorner = edgeDist < blendWidth;

                // do 1, 2 or 3 biome blend based on proximity to edge/corner
                if (!nearEdge)
                {
                    // Pure biome
                    weight = 1f;
                    secondWeight = 0f;
                    thirdWeight = 0f;
                }
                else if (!nearCorner)
                {
                    /*
                    // 2-biome blend
                    float sum = w1 + w2;
                    weight = w1 / sum;
                    secondWeight = w2 / sum;
                    thirdWeight = 0f;
                    */
                    // Distance-based blend from 1 → 0.5
                    float t = Mathf.Clamp01(edgeDist / blendWidth);

                    weight = Mathf.Lerp(0.5f, 1f, t);
                    secondWeight = 1f - weight;
                    thirdWeight = 0f;
                }
                else
                {
                    // 3-biome blend (corner)
                    /*
                    float sum = w1 + w2 + w3;
                    weight = w1 / sum;
                    secondWeight = w2 / sum;
                    thirdWeight = w3 / sum;
                    */
                    float t = Mathf.Clamp01(edgeDist / blendWidth);

                    weight = Mathf.Lerp(0.5f, 1f, t);
                    float t2 = 1f - weight;
                    secondWeight = t2 * (w2 / (w2 + w3));
                    thirdWeight = t2 * (w3 / (w2 + w3));
                }

                
                // Smoothstep weights for better blending
                weight = weight * weight * (3f - 2f * weight);
                secondWeight = secondWeight * secondWeight * (3f - 2f * secondWeight);
                thirdWeight = thirdWeight * thirdWeight * (3f - 2f * thirdWeight);
                

                BiomeCoord newBiomeCoord = new BiomeCoord(pointBiomeMap[closestCell.x, closestCell.y], weight, pointBiomeMap[secondClosestCell.x, secondClosestCell.y], secondWeight, pointBiomeMap[thirdClosestCell.x, thirdClosestCell.y], thirdWeight);
                // closest biome only for pure biome borders
                //BiomeCoord newBiomeCoord = new BiomeCoord(pointBiomeMap[closestCell.x, closestCell.y], 1, pointBiomeMap[secondClosestCell.x, secondClosestCell.y], 0);
                
                biomeMap[x, y] = newBiomeCoord;
                //Debug.Log("Assigned biome: " + newBiomeCoord.getBiome() + " at (" + x + "," + y + ") with weight " + newBiomeCoord.getWeight() + " and second biome: " + newBiomeCoord.getSecondBiome() + " with weight " + newBiomeCoord.getSecondWeight());
            }
        }
        buildingPointsArray = GeneratePoints(numOfCells, pixelsPerCell, prng, 3); // Generate building points to be used later
        BiomeGenData biomeGenData = new BiomeGenData(biomeMap, buildingPointsArray);
        return biomeGenData;
    }

    // COME BACK AND OPTIMIZE - MOST LIKELY GENERATE IN METHOD ABOVE

    // modify to make more general, ok for now but for buildings need to have more parameters - e.g. only gen building in 1 weights of biome
    private static Vector2Int[,] GeneratePoints(int cells, int pixelsPerCell, System.Random prng, int n){
        // return array of random points positions within each cell, n = number of points to generate; use for biome spawning, other things such as buildings etc.
        Vector2Int[,] pointsPosArray = new Vector2Int[cells, cells];
        for (int i = 0; i < cells; i++){
            for (int j = 0; j < cells; j++){
                for (int k = 0; k < n; k++){
                    //pointsPosArray[i, j] = new Vector2Int(i * pixelsPerCell + Random.Range(0, pixelsPerCell), j * pixelsPerCell + Random.Range(0, pixelsPerCell)); // Each point is a random position within its cell
                    pointsPosArray[i, j] = new Vector2Int(i * pixelsPerCell + prng.Next(0, pixelsPerCell), j * pixelsPerCell + prng.Next(0, pixelsPerCell)); // Each point is a random position within its cell
                }
            }
        }
        return pointsPosArray;
    }

    // potential optimization: use enum for biome types instead of string names
    private static string [,] GeneratePointBiomes(int cells, int pixelsPerCell, BiomeScriptableObject[] biomes, System.Random prng){
        string [,] pointBiomeMap = new string[cells, cells];
        for (int i = 0; i < cells; i++)
        {
            for (int j = 0; j < cells; j++)
            {
                //cellColorsArray[i, j] = cellColours[Random.Range(0, cellColours.Length)];// Assign a random color from the array
                //cellColorsArray[i, j] = cellColours[prng.Next(0, cellColours.Length)];// Assign a random color from the array
                pointBiomeMap[i,j] = biomes[prng.Next(0, biomes.Length)].name;// Create new biomeCoord object, assign random biome 
            }
        }
        return pointBiomeMap;
    }
}

public class BiomeCoord{
    // holds biome type, weight for a given coordinate of the two closest biomes
    string biome;
    float weight;
    string secondBiome;
    float secondWeight;
    string thirdBiome;
    float thirdWeight;

    public BiomeCoord (string _biome, float _weight, string _sBiome, float _sWeight, string _tBiome, float _tWeight){
        biome = _biome;
        weight = _weight;
        secondBiome = _sBiome;
        secondWeight = _sWeight;
        thirdBiome = _tBiome;
        thirdWeight = _tWeight;
    }

    public string getBiome(){return biome;}
    public float getWeight(){return weight;}
    public string getSecondBiome(){return secondBiome;}
    public float getSecondWeight(){return secondWeight;}
    public string getThirdBiome(){return thirdBiome;}
    public float getThirdWeight(){return thirdWeight;}
}

public struct BiomeGenData{
    public BiomeCoord[,] voronoiMap;
    public Vector2Int[,] buildingPointsArray;


    public BiomeGenData(BiomeCoord[,] voronoiMap, Vector2Int[,] buildingPointsArray){
        this.voronoiMap = voronoiMap;
        this.buildingPointsArray = buildingPointsArray;
        }
    }
