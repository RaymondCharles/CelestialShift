using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLeaf : MonoBehaviour
{
    public float rotationSpeed = 30f;

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
    }
}
