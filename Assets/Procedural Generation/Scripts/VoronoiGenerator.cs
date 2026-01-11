using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO
// 1. Add Noise to edges DONE
// 2. Change to make voronoi generic, i.e. return VoronoiMap of floats, then move colour logic to perlin one DONEaaaa
// 3. Optimise points generation code - generate in main method or see if needed for other return
// 4. Maybe tie noiseScale to no of cells
// 5. Add biome warpFreq and warpStrength parameters, blendWidth parameter for biome blending

// OPTMIZATION IDEAS
// 1. In seedBiome and Voronoi seed position, move away from using Random class, use hash function to generate random values based on cell coords and seed
public static class VoronoiGenerator{
    public static BiomeGenData GenerateVDiagram(int chunkWidth, int chunkHeight, Vector2 offset, int biomeSize, int seed, BiomeScriptableObject[] biomes, float blendWidth, float warpStrength)
    {
        
        // Create ColourMap, loop through pixels, assign colour according to Voronoi logic
        // Ensure we have at least one cell and at least one pixel per cell
        int cells = Mathf.Max(1, biomeSize);
        int pixelsPerCell = Mathf.Max(1, chunkWidth / cells);

        System.Random prng = new System.Random (seed);

        // Create Maps to be returned
        BiomeCoord[,] biomeMap = new BiomeCoord [chunkWidth , chunkHeight];
        Vector2Int[,] buildingPointsArray = new Vector2Int[biomeSize, biomeSize];

        float warpFreq = (float)biomeSize / (float)chunkWidth;
        //float warpStrength = biomeSize / 2f;

        float closestDist;
        float secondClosestDist;
        float thirdClosestDist;
        Vector2Int closestCell;
        Vector2Int secondClosestCell;
        Vector2Int thirdClosestCell;
        string closestBiome = "";
        string secondClosestBiome = "";
        string thirdClosestBiome = "";

        int cellSize = pixelsPerCell;

        int originX = Mathf.RoundToInt(offset.x);
        int originY = Mathf.RoundToInt(offset.y);

        // chunk spans worldX: [offset.x, offset.x + chunkWidth)
        // same for Y
        int minCellX = FloorDiv(originX, cellSize) - 2;
        int minCellY = FloorDiv(originY, cellSize) - 2;

        int maxCellX = FloorDiv(originX + (chunkWidth  - 1), cellSize) + 2;
        int maxCellY = FloorDiv(originY + (chunkHeight - 1), cellSize) + 2;

        int cellsX = maxCellX - minCellX + 1;
        int cellsY = maxCellY - minCellY + 1;

        Vector2Int[,] seedPos = new Vector2Int[cellsX, cellsY];
        string[,] biomeLookup = new string[cellsX, cellsY];

        for (int cx = 0; cx < cellsX; cx++) {
            for (int cy = 0; cy < cellsY; cy++) {

                int cellX = minCellX + cx;
                int cellY = minCellY + cy;

                seedPos[cx, cy] = VoronoiSeedPosition(cellX, cellY, seed, pixelsPerCell);
                biomeLookup[cx, cy] = SeedBiome(cellX, cellY, seed, biomes);
            }
        }
        
        // Loop through each pixel to determine its closest point, and assign color accordingly
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                // Reset closest distance and cell for each pixel
                closestDist = Mathf.Infinity;
                secondClosestDist = Mathf.Infinity;
                thirdClosestDist = Mathf.Infinity;
                closestCell = new Vector2Int();
                secondClosestCell = new Vector2Int();
                thirdClosestCell = new Vector2Int();

                // add noise before biome assignment to avoid straight lines
                int wx = originX + x;
                int wy = originY + y;

                float nx = Mathf.PerlinNoise(wx * warpFreq, wy * warpFreq) - 0.5f;
                float ny = Mathf.PerlinNoise((wx + 1000f) * warpFreq, (wy + 1000f) * warpFreq) - 0.5f;

                Vector2 warpedSample = new Vector2(
                            x + offset.x + nx * warpStrength,
                            y + offset.y + ny * warpStrength
                        );

                // Get the world grid position of the current pixel
                int gridX = Mathf.FloorToInt(warpedSample.x / pixelsPerCell);
                int gridY = Mathf.FloorToInt(warpedSample.y / pixelsPerCell);
                
                int r = 1 + Mathf.CeilToInt(warpStrength / pixelsPerCell);

                for (int i = -r; i <= r; i++){
                    for (int j = -r; j <= r; j++)
                    {
                        int X = gridX + i;
                        int Y = gridY + j;

                        int lx = X - minCellX;
                        int ly = Y - minCellY;

                        if (lx < 0 || lx >= cellsX || ly < 0 || ly >= cellsY) continue;

                        var currentSeed = seedPos[lx, ly];
                        var currentBiome = biomeLookup[lx, ly];

                        float distance = Vector2.Distance(warpedSample, currentSeed);

                        // Once loop exits, we have the closest 2 cells, atm just using 2 closest for blending - 3 is broken, can fix later
                        if (distance < closestDist)
                        {
                            // shift closest → second, second → third
                            thirdClosestDist  = secondClosestDist;
                            thirdClosestCell  = secondClosestCell;
                            thirdClosestBiome = secondClosestBiome;

                            secondClosestDist  = closestDist;
                            secondClosestCell  = closestCell;
                            secondClosestBiome = closestBiome;

                            closestDist  = distance;
                            closestCell  = currentSeed;
                            closestBiome = currentBiome;
                        }
                        else if (distance < secondClosestDist)
                        {
                            // shift second → third
                            thirdClosestDist  = secondClosestDist;
                            thirdClosestCell  = secondClosestCell;
                            thirdClosestBiome = secondClosestBiome;

                            secondClosestDist  = distance;
                            secondClosestCell  = currentSeed;
                            secondClosestBiome = currentBiome;
                        }
                        else if (distance < thirdClosestDist)
                        {
                            thirdClosestDist  = distance;
                            thirdClosestCell  = currentSeed;
                            thirdClosestBiome = currentBiome;
                        }
                    }
                }
                
                float weight = 1f;
                float secondWeight = 0f;
                float thirdWeight = 0f;

                // inverse distances to get weights (closer = higher weight) 
                weight = 1f / (closestDist + 0.0001f);
                secondWeight = 1f / (secondClosestDist + 0.0001f); 
                thirdWeight = 1f / (thirdClosestDist + 0.0001f);

                // determine if near edge or corner
                float edgeDist = secondClosestDist - closestDist;
                float cornerDist = thirdClosestDist - closestDist;

                bool nearEdge   = edgeDist   < blendWidth;
                bool nearCorner = cornerDist < blendWidth;
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
                    // 2 biome blend
                    // Distance-based blend from 1 → 0.5
                    float t = Mathf.Clamp01(edgeDist / blendWidth);

                    t = t * t * (3f - 2f * t); // smoothstep

                    weight = Mathf.Lerp(0.5f, 1f, t);
                    secondWeight = 1f - weight;
                    thirdWeight = 0f;
                }
                else
                {
                    // 3 biome blend
                    float t = Mathf.Clamp01(edgeDist / blendWidth);

                    weight = Mathf.Lerp(0.3333f, 1f, t);
                    float t2 = 1f - weight;
                    float sum = secondWeight + thirdWeight;
                    secondWeight = t2 * (secondWeight / sum);
                    thirdWeight = t2 * (thirdWeight / sum);
                }

                /*
                // Smoothstep weights for better blending
                weight = weight * weight * (3f - 2f * weight);
                secondWeight = secondWeight * secondWeight * (3f - 2f * secondWeight);
                thirdWeight = thirdWeight * thirdWeight * (3f - 2f * thirdWeight);
                */

                // Renormalize
                float sum = weight + secondWeight + thirdWeight;
                if (sum > 0f) {
                    weight /= sum;
                    secondWeight /= sum;
                    thirdWeight /= sum;
                }
                

                BiomeCoord newBiomeCoord = new BiomeCoord(closestBiome, weight, secondClosestBiome, secondWeight, thirdClosestBiome, thirdWeight);
                // closest biome only for pure biome borders
                //BiomeCoord newBiomeCoord = new BiomeCoord(pointBiomeMap[closestCell.x, closestCell.y], 1, pointBiomeMap[secondClosestCell.x, secondClosestCell.y], 0);
                
                biomeMap[x, y] = newBiomeCoord;
                //Debug.Log("Assigned biome: " + newBiomeCoord.getBiome() + " at (" + x + "," + y + ") with weight " + newBiomeCoord.getWeight() + " and second biome: " + newBiomeCoord.getSecondBiome() + " with weight " + newBiomeCoord.getSecondWeight() + " and third biome: " + newBiomeCoord.getThirdBiome() + " with weight " + newBiomeCoord.getThirdWeight());
                //if ((x % 64 == 0) && (y % 64 == 0)) {Debug.Log("Assigned biome: " + newBiomeCoord.getBiome() + " at (" + x + "," + y + ") with weight " + newBiomeCoord.getWeight() + " and second biome: " + newBiomeCoord.getSecondBiome() + " with weight " + newBiomeCoord.getSecondWeight());}
            }
        }
        buildingPointsArray = GeneratePoints(biomeSize, pixelsPerCell, prng, 3, Vector2Int.zero); // Generate building points to be used later
        BiomeGenData biomeGenData = new BiomeGenData(biomeMap, buildingPointsArray);
        return biomeGenData;
    }

    // COME BACK AND OPTIMIZE - MOST LIKELY GENERATE IN METHOD ABOVE

    // modify to make more general, ok for now but for buildings need to have more parameters - e.g. only gen building in 1 weights of biome
    private static Vector2Int[,] GeneratePoints(int cells, int pixelsPerCell, System.Random prng, int n, Vector2 offset){
        // return array of random points positions within each cell, n = number of points to generate; use for biome spawning, other things such as buildings etc.
        // i represents world position, x and y represent chunk position
        Vector2Int[,] pointsPosArray = new Vector2Int[cells, cells];
        for (int i = 0; i < cells; i++){
            for (int j = 0; j < cells; j++){
                for (int k = 0; k < n; k++){
                    //pointsPosArray[i, j] = new Vector2Int(i * pixelsPerCell + Random.Range(0, pixelsPerCell), j * pixelsPerCell + Random.Range(0, pixelsPerCell)); // Each point is a random position within its cell
                    pointsPosArray[i, j] = new Vector2Int(i  * pixelsPerCell + prng.Next(0, pixelsPerCell), j * pixelsPerCell + prng.Next(0, pixelsPerCell)); // Each point is a random position within its cell
                }
            }
        }
        return pointsPosArray;
    }

    private static Vector2Int VoronoiSeedPosition(int cellX, int cellY, int seed, int pixelsPerCell)
    {
        unchecked
        {
            int hash = seed
                ^ (cellX * 73856093)
                ^ (cellY * 19349663);

            var prng = new System.Random(hash);

            int x = cellX * pixelsPerCell + prng.Next(0, pixelsPerCell);
            int y = cellY * pixelsPerCell + prng.Next(0, pixelsPerCell);

            return new Vector2Int(x, y);
        }
    }

    private static string SeedBiome(int cellX, int cellY, int seed, BiomeScriptableObject[] biomes){
        // MUST CHANGE TO INDEX OR ENUM LATER ON
        // for this and above - find new approach not using random

        int hash = seed
         ^ (cellX * 73856093)
         ^ (cellY * 19349663);

        System.Random prng = new System.Random(hash);

        string biome = biomes[prng.Next(0, biomes.Length)].name;

        return biome;
    }

    static int FloorDiv(int a, int b) {
    int q = a / b;
    int r = a % b;
    if (r != 0 && ((r > 0) != (b > 0))) q--;
    return q;
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
