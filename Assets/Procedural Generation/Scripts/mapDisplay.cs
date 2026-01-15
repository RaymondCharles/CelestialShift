using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture){
        // Renders Texture
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width,1,texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture){
        // Creates mesh from meshdata, applies texture
        meshFilter.sharedMesh = meshData.CreateMesh ();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
