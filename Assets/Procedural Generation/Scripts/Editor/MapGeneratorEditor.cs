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
                mapGen.GenerateMapDataInEditor();
                mapGen.DrawMapInEditor();
            }
        }   
        if (GUILayout.Button ("Generate")){
            mapGen.DrawMapInEditor();
        }
        if (GUILayout.Button ("Generate Map Data")){
            mapGen.GenerateMapDataInEditor();
        }
    }
}
