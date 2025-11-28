using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/BiomeScriptableObject", order = 1)]
public class BiomeScriptableObject : ScriptableObject
{
    public string name;
    public TerrainType[] terrainType;
    public float noiseScale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public Color biomeColour;
}