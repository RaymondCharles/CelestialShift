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
// 6. PixelsPerCell, change to just chunkSize as we use this

// OPTMIZATION IDEAS
// 1. In seedBiome and Voronoi seed position, move away from using Random class, use hash function to generate random values based on cell coords and seed
public static class VoronoiGenerator{
    public static BiomeGenData GenerateVDiagram(int chunkWidth, int chunkHeight, Vector2 offset, int biomeSize, int seed, BiomeScriptableObject[] biomes, float blendWidth, float warpStrength)
    {
        // Ensure we have at least one cell and at least one pixel per cell
        int cells = Mathf.Max(1, biomeSize);
        int pixelsPerCell = Mathf.Max(1, chunkWidth / cells);

        Vector2Int currSeedPos;

        // pseud random number generator using seed for deterministic results
        System.Random prng = new System.Random (seed);

        // Create Map, dungeonArray to be returned
        BiomeCoord[,] biomeMap = new BiomeCoord [chunkWidth , chunkHeight];
        List<Vector2Int> dungeonArray = new List<Vector2Int>();

        float warpFreq = 1f / pixelsPerCell;
        //float warpStrength = biomeSize / 2f;

        // track 3 closest cell biomes and distances for blending
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

        // tracks world origin of chunk - we sample in world space and construct map based on chunkspace
        float originX = offset.x;
        float originY = -offset.y;

        int r = 1 + Mathf.CeilToInt(warpStrength / cellSize);
        int pad = r + 2;

        int minCellX = Mathf.FloorToInt(originX / cellSize) - pad;
        int minCellY = Mathf.FloorToInt(originY / cellSize) - pad;

        int maxCellX = Mathf.FloorToInt((originX + (chunkWidth  - 1)) / cellSize) + pad;
        int maxCellY = Mathf.FloorToInt((originY + (chunkHeight - 1)) / cellSize) + pad;

        int cellsX = maxCellX - minCellX + 1;
        int cellsY = maxCellY - minCellY + 1;

        // precompute seed positions, biomes and dungeon positions for each cell in range
        Vector2Int[,] seedPos = new Vector2Int[cellsX, cellsY];
        Vector2Int[,] dungeonPos = new Vector2Int[cellsX, cellsY];
        string[,] biomeLookup = new string[cellsX, cellsY];
        float dungeonRange = 0.2f * biomeSize;

        for (int cx = 0; cx < cellsX; cx++) {
            for (int cy = 0; cy < cellsY; cy++) {

                int cellX = minCellX + cx;
                int cellY = minCellY + cy;

                currSeedPos = VoronoiSeedPosition(cellX, cellY, seed, pixelsPerCell);
                seedPos[cx, cy] = currSeedPos;
                biomeLookup[cx, cy] = SeedBiome(cellX, cellY, seed, biomes);
                dungeonPos[cx, cy] = new Vector2Int(
                    // dungeons placed randomly in a 30x30 area around the seed point
                    currSeedPos.x + prng.Next(0, (int)dungeonRange),
                    currSeedPos.y + prng.Next(0, (int)dungeonRange)
                );
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

                // world position of current pixel
                float wx = originX + x;
                float wy = originY + y;

                // add noise before biome assignment to avoid straight lines
                float nx = Mathf.PerlinNoise(wx * warpFreq, wy * warpFreq) - 0.5f;
                float ny = Mathf.PerlinNoise((wx + 1000f) * warpFreq, (wy + 1000f) * warpFreq) - 0.5f;

                Vector2 warpedSample = new Vector2(
                            x + originX + nx * warpStrength,
                            y + originY + ny * warpStrength
                        );

                // Get the world grid position of the current pixel
                int gridX = Mathf.FloorToInt(warpedSample.x / pixelsPerCell);
                int gridY = Mathf.FloorToInt(warpedSample.y / pixelsPerCell);

                // Check neighboring cells within range r to find the closest seed point
                for (int i = -r; i <= r; i++){
                    for (int j = -r; j <= r; j++)
                    {
                        int X = gridX + i;
                        int Y = gridY + j;

                        int lx = X - minCellX;
                        int ly = Y - minCellY;

                        if (lx < 0 || lx >= cellsX || ly < 0 || ly >= cellsY) continue;

                        // Get the seed position and biome of the current cell
                        var currentSeed = seedPos[lx, ly];
                        var currentBiome = biomeLookup[lx, ly];

                        float distance = Vector2.Distance(warpedSample, currentSeed);

                        // Once loop exits, we have the closest 3 cells, we use these to determine biome and blending
                        if (distance < closestDist)
                        {
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
                
                // if this pixel is the dungeon position for the closest cell, assign dungeon position to dungeon array
                // only one dungeon per cell, so only need to check closest cell, only around point so not possible for a single pixel to be dungeon for any cell other than closest
                if (wx == closestCell.x && wy == closestCell.y) {
                    int dungeonX = closestCell.x / pixelsPerCell;
                    int dungeonY = closestCell.y / pixelsPerCell;
                    dungeonArray.Add(dungeonPos[(closestCell.x - minCellX * pixelsPerCell) / pixelsPerCell, (closestCell.y - minCellY * pixelsPerCell) / pixelsPerCell]);
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
                    float sum23 = secondWeight + thirdWeight;
                    secondWeight = t2 * (secondWeight / sum23);
                    thirdWeight = t2 * (thirdWeight / sum23);
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
        BiomeGenData biomeGenData = new BiomeGenData(biomeMap, dungeonArray);
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
        // MUST CHANGE biome TO INDEX OR ENUM LATER ON
        // for this and above - find new approach not using random

        int hash = seed
         ^ (cellX * 73856093)
         ^ (cellY * 19349663);

        System.Random prng = new System.Random(hash);

        string biome = biomes[prng.Next(0, biomes.Length)].name;

        return biome;
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
    public List<Vector2Int> dungeonArray;


    public BiomeGenData(BiomeCoord[,] voronoiMap, List<Vector2Int> dungeonArray){
        this.voronoiMap = voronoiMap;
        this.dungeonArray = dungeonArray;
        }
    }
