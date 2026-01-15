using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCameraPosition : MonoBehaviour
{
    public Transform headBone;

    void LateUpdate()
    {
        transform.position = headBone.position;
    }
}
