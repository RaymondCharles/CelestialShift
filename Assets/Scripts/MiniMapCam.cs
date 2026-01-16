using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCam : MonoBehaviour
{

    public Transform player;
    public float height = 30.0f;
   

    void LateUpdate()
    {
        Vector3 newPos = player.position;
        newPos.y = height;
        transform.position = newPos;
    }
}
