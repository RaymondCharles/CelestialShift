using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpawner : MonoBehaviour
{
    public GameObject tilePrefab;
    public int width = 10;
    public int height = 10;

    void Start()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Instantiate(
                    tilePrefab,
                    new Vector3(x, 0, z),
                    Quaternion.identity,
                    transform
                );
            }
        }
    }
}