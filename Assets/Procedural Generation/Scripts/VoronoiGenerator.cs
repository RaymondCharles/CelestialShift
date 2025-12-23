using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO
// 1. Add Noise to edges
// 2. Change to make voronoi generic, i.e. return VoronoiMap of floats, then move colour logic to perlin one
// 3. Optimise points generation code - generate in main method or see if needed for other return
// 4. Maybe tie noiseScale to no of cells
public static class VoronoiGenerator{
    public static string [,] GenerateVDiagram(int width, int height, Color[] cellColours, int numOfCells, int seed, BiomeScriptableObject[] biomes)
    {
        // Create ColourMap, loop through pixels, assign colour according to Voronoi logic
        // Ensure we have at least one cell and at least one pixel per cell
        int cells = Mathf.Max(1, numOfCells);
        int pixelsPerCell = Mathf.Max(1, width / cells);

        System.Random prng = new System.Random (seed);

        // Create Colour Map
        //Color[] colourMap = new Color[width * height];

        // Create BiomeMap
        string [,] biomeMap = new string [width , height];

        // Generate Points and CellColours array
        Vector2Int[,] pointsPosArray = GeneratePoints(numOfCells, pixelsPerCell, prng); // Array to hold cell point positions
        string [,] pointBiomeMap = GeneratePointBiomes(numOfCells, pixelsPerCell, biomes, prng);// Dictionary to hold biome assignment for each point

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

        // Loop through each pixel to determine its closest point, and assign color accordingly
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Get the grid position of the current pixel
                int gridX = x / pixelsPerCell;
                int gridY = y / pixelsPerCell;

                float closestDist = Mathf.Infinity;
                Vector2Int closestCell = new Vector2Int();

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        // Calculate the pixel coordinates
                        int X = gridX + i;
                        int Y = gridY + j;
                        // Check if the pixel is within bounds
                        if (X < 0 || Y < 0 || X >= numOfCells || Y >= numOfCells) continue;

                        // Create Vector for distance calculation
                        float distance = Vector2Int.Distance(new Vector2Int(x, y), pointsPosArray[X, Y]);

                        // Once loop exits, we have the closest cell
                        if (distance < closestDist)
                        {
                            closestDist = distance;
                            closestCell = new Vector2Int(X, Y);
                        }
                    }
                }
                // Once looped through all nearby points, assign color of closest cell
                //colourMap[y * width + x] = cellColorsArray[closestCell.x, closestCell.y];
                biomeMap[x, y] = pointBiomeMap[closestCell.x, closestCell.y];
            }
        }
        return biomeMap;
    }

    // COME BACK AND OPTIMIZE - MOST LIKELY GENERATE IN METHOD ABOVE
private static Vector2Int[,] GeneratePoints(int cells, int pixelsPerCell, System.Random prng){
        Vector2Int[,] pointsPosArray = new Vector2Int[cells, cells];
        for (int i = 0; i < cells; i++)
        {
            for (int j = 0; j < cells; j++)
            {
                //pointsPosArray[i, j] = new Vector2Int(i * pixelsPerCell + Random.Range(0, pixelsPerCell), j * pixelsPerCell + Random.Range(0, pixelsPerCell)); // Each point is a random position within its cell
                pointsPosArray[i, j] = new Vector2Int(i * pixelsPerCell + prng.Next(0, pixelsPerCell), j * pixelsPerCell + prng.Next(0, pixelsPerCell)); // Each point is a random position within its cell
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

public class biomeCoord{
    string biome;

    public biomeCoord(float _x, float _y, string _biome){
        biome = _biome;
    }

    public string getBiome(){
        return biome;
    }
}


