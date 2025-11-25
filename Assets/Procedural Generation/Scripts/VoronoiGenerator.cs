using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class VoronoiGenerator{
    /*
    [SerializeField] private Color[] cellColors;
    [SerializeField] private int numOfCells = 10;
    private int imgSize;
    private int pixelsPerCell;
    private RawImage image;
    private Vector2Int[,] pointsPosArray; // Array to hold cell point positions
    private Color[,] cellColorsArray; // Array to hold cell colors

    
    private void Awake()
    {
        // Fetch RawImage component and determine size
        // Just cache the RawImage. The RectTransform size may not be final at Awake
        image = GetComponent<RawImage>();
    }

    
    // Check that colors are assigned in the inspector
    private void OnValidate()
    {
        if (cellColors == null || cellColors.Length == 0)
        {
            cellColors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta };
        }
    }

    
    private void Start()
    {
        // Force layout rebuild so RectTransform.rect has the correct pixel size
        var rt = image.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        imgSize = Mathf.RoundToInt(rt.rect.width);
        if (imgSize <= 0)
        {
            Debug.LogWarning($"VoronoiDiagram: computed imgSize <= 0 (rect.width={rt.rect.width}). Using fallback 256.");
            imgSize = 256; // fallback to a sane default
        }

        GenerateVDiagram();
    }
    */

    public static Color[] GenerateVDiagram(int width, int height, Color[] cellColours, int numOfCells)
    {
        // Create texture, loop through pixels, assign colour according to Voronoi logic
        // Ensure we have at least one cell and at least one pixel per cell
        int cells = Mathf.Max(1, numOfCells);
        int pixelsPerCell = Mathf.Max(1, width / cells);

        // Create Colour Map
        Color[] colourMap = new Color[width * height];

        // Ensure points are generated before use
        Vector2Int[,] pointsPosArray = GeneratePoints(numOfCells, pixelsPerCell); // Array to hold cell point positions
        Color[,] cellColorsArray = GeneratePointColours(numOfCells, pixelsPerCell, cellColours);

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
                //texture.SetPixel(x, y, cellColorsArray[closestCell.x, closestCell.y]);
                colourMap[x * width + y] = cellColorsArray[closestCell.x, closestCell.y];
            }
        }
        /*
        texture.Apply();
        image.texture = texture;
        // Save a PNG to persistentDataPath
        byte[] png = texture.EncodeToPNG();
        string path = System.IO.Path.Combine(Application.persistentDataPath, $"voronoi_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        System.IO.File.WriteAllBytes(path, png);
        */
        return colourMap;
    }

    // COME BACK AND OPTIMIZE - MOST LIKELY GENERATE IN METHOD ABOVE
private static Vector2Int[,] GeneratePoints(int cells, int pixelsPerCell){
        Vector2Int[,] pointsPosArray = new Vector2Int[cells, cells];
        for (int i = 0; i < cells; i++)
        {
            for (int j = 0; j < cells; j++)
            {
                pointsPosArray[i, j] = new Vector2Int(i * pixelsPerCell + Random.Range(0, pixelsPerCell), j * pixelsPerCell + Random.Range(0, pixelsPerCell)); // Each point is a random position within its cell
            }
        }
        return pointsPosArray;
    }

    private static Color[,] GeneratePointColours(int cells, int pixelsPerCell, Color[] cellColours){
        Color[,] cellColorsArray = new Color[cells, cells];
        for (int i = 0; i < cells; i++)
        {
            for (int j = 0; j < cells; j++)
            {
                cellColorsArray[i, j] = cellColours[Random.Range(0, cellColours.Length)];// Assign a random color from the array
            }
        }
        return cellColorsArray;
    }
}
