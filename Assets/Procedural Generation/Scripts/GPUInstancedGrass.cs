using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUInstancedGrass : MonoBehaviour
{
    [SerializeField] Mesh grassMesh;
    [SerializeField] Material grassMaterial;

    [SerializeField] public int size = 10;

    private Matrix4x4[] matrices;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        matrices = new Matrix4x4[size * size * size];
    
    int i = 0;
    for (int x = 0; x < size; x++)
    {
        for (int y = 0; y < size; y++)
        {
            for (int z = 0; z < size; z++)
            {
                Vector3 position = new Vector3(x * 2.0f, y * 2.0f, z * 2.0f);
                matrices[i] = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                i++;
            }
        }
    }
    Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, matrices);
    }

    private void RenderGrassGPUI(float[,] noiseMap, BiomeCoord[,] biomeMap, Bounds bounds)
    {
        // using a list of matrix lists to batch draw calls
        List<List<Matrix4x4>> matrices = new List<List<Matrix4x4>>();

        int batch = 0;
        List<Matrix4x4> newMatrixList = new List<Matrix4x4>();
        matrices.Add(newMatrixList);

        for (float x = bounds.center.x - bounds.size.x / 2; x < bounds.center.x + bounds.size.x / 2; x += 0.4f)
        {
            for (float z = bounds.center.z - bounds.size.z / 2; z < bounds.center.z + bounds.size.z / 2; z += 0.4f)
            {
                if  (biomeMap[(int)x, (int)z].getBiome() == "Grass Plains"){ 
                    float y = noiseMap[(int)x, (int)z];
                    Vector3 position = new Vector3(x, y, z);
                    Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                    matrices[batch].Add(matrix);
                }
                if (matrices[batch].Count >= 1000)// do not exceed matrix limit
                {
                    batch++;
                    matrices.Add(new List<Matrix4x4>());
                }
            }
        }
        
        for (int i = 0; i < batch; i++)
        {
            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, matrices[i]);
        }
    }
}
