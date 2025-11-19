using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor (typeof (mapGenerator))]
public class MapGeneratorEditor : Editor {
    public override void OnInspectorGui(){
        mapGenerator mapGen = (mapGenerator)target;

        // draw default inspector, adding a button to generate map
        DrawDefaultInspector ();
        if (GUILayout.Button ("Generate")){
            mapGen.GenerateMap();
        }
    }
}
