using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public bool autoUpdate;

    public void generateMap(){
        // call noise.GenerateNoiseMap() with parameters to generate noise map
        float[,] noiseMap = noise.GenerateNoiseMap (mapWidth, mapHeight, noiseScale);

        // find displayMap object, and draw noisemap
        mapDisplay display = FindObjectOfType<mapDisplay> ();
        display.DrawNoiseMap (noiseMap);

    }
}
