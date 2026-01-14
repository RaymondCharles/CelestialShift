using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treeGenerator : MonoBehaviour
{
    public TreeCoord[] AssignTreePos(int chunkSize, int seed, float offset, float[,] heightMap, Dictionary<string, BiomeScriptableObject> biomeDict, BiomeCoord[,] biomeMap){
        List<TreeCoord> treeCoords = new List<TreeCoord>();
        System.Random prng = new System.Random(seed + (int)offset);
        int x = 0;
        int y = 0;
        // need to match voronoi biome y 
        foreach (string biome in biomeDict.Keys){
            for (int objIndex = 0; objIndex < biomeDict[biome].treePrefabs.Length; objIndex++){
                for (int i = 0; i < biomeDict[biome].treePrefabs[objIndex].countPerBiome; i++){
                    for (int j = 0; j < 20; j++){ // try 20 times to find a valid location - avoids infinite loop for no biome areas - need to improve i.e. what if biome area is smaller than number of objects to place
                        x = prng.Next(0, chunkSize);
                        y = prng.Next(0, chunkSize);
                        // check biome at this location matches
                        if (biomeMap[x,y].getBiome() == biome){
                            treeCoords.Add(new TreeCoord(biomeDict[biome].treePrefabs[objIndex].name, x, y, objIndex, biomeDict[biome]));
                            break;
                        }
                    }
                }
            }
        }
        return treeCoords.ToArray();
    }
}

public class TreeCoord{
    // struct to hold tree co-ordinates, biome type so lookup doesnt need to be done again at runtime
    public string name;
    public int x;
    public int y;
    public int z;
    public int objectIndex;
    public BiomeScriptableObject biomeType;

    public TreeCoord(string name, int x, int y, int objectIndex, BiomeScriptableObject biomeType){
        this.name = name;
        this.x = x;
        this.y = y;
        this.objectIndex = objectIndex;
        this.biomeType = biomeType;
    }
}
