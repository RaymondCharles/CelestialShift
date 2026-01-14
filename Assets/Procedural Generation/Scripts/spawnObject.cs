using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class spawnObject{
    public static void SpawnObjectAtPoint(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Transform parentTransform){
    GameObject obj = UnityEngine.Object.Instantiate(prefab);

    // set parent
    obj.transform.SetParent(parentTransform, false);
    // apply local transform
    obj.transform.localPosition = localPosition;
    obj.transform.localRotation = localRotation;

    Debug.Log(
        $"Spawned object '{obj.name}'\n" +
        $"Local Position: {obj.transform.localPosition}\n" +
        $"World Position: {obj.transform.position}\n" +
        $"Parent: {parentTransform.name}"
    );
    }
}