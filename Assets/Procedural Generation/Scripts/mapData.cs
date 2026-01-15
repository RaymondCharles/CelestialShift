using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MapDataScriptableObject", order = 2)]
public class MapDataScriptableObject : ScriptableObject
{
    // Scriptable Object to hold map data
    public MapData mapData;
}