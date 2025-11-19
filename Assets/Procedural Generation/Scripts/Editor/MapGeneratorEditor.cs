using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (mapGenerator))]
public class MapGeneratorEditor : Editor {
    public override void OnInspectorGUI(){
        mapGenerator mapGen = (mapGenerator)target;

        // draw default inspector, adding a button to generate map
        if (DrawDefaultInspector()){
            if (mapGen.autoUpdate){
                mapGen.generateMap();
            }
        }   
        if (GUILayout.Button ("Generate")){
            mapGen.generateMap();
        }
    }
}
