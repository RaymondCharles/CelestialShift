using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]//ensures there is always a mesh filter on the object
public static class meshGenerator
{

    public static MeshData GenerateTerrainMesh(float[,] heightMap){
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width-1) / -2f;
        float topLeftZ = (height-1) / 2f;
    
        MeshData meshData = new MeshData (width, height);
        int vertexIndex = 0;

        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightMap[x,y], topLeftX - y);
                meshData.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height);

                if (x < width - 1 && y < height -1 ){
                    // set the 2 triangles per quad, skipping right and bottom edge to ensure erroneous triangles are not drawn, triangles are set clockwise to ensure unitys lighting correctly renders
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }
        return meshData;
    }
}

//Mesh Data Class containing vertices and triangles array, and a helper method to add a triangle
public class MeshData{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    // 1D arrays are used to store both triangles and vertices.
    public MeshData(int meshWidth, int meshHeight){
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth-1)*(meshHeight-1)];
    }

    public void AddTriangle(int a, int b, int c){
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    // create mesh method, adds vertices, triangles and uv map to mesh, recalculates normals to ensure correct lighting
    public Mesh CreateMesh(){
        Mesh mesh = new Mesh ();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals ();
        return mesh;
    }
}