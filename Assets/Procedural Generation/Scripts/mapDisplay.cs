using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;

    public void DrawNoiseMap(float[,] noiseMap){
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D (width, height);

        // 1d array to store colours for noisemap in order to get a visual representation
        Color [] colourMap = new Color[width * height];
        for (int y=0; y < height; y++){
            for (int x=0; x < height; x++){
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap [x, y]);
            }
        }
        texture.SetPixels (colourMap);
        texture.Apply ();

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(width,1,height);
    }
}
