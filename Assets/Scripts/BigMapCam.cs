using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class BigMapCam : MonoBehaviour
{
    public Transform player;
    public float height = 600f;
    public float zoomSpeed = 50f;
    public float minHeight = 200f;
    public float maxHeight = 900f;
    private float scroll;

    void LateUpdate()
    {
        HandleZoom();

        Vector3 newPos = player.position;
        newPos.y = height;
        transform.position = newPos;
    }

    void HandleZoom()
    {
        if (Mouse.current == null) return;

         scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0f)
        {
            height -= scroll * zoomSpeed * Time.deltaTime;
            height = Mathf.Clamp(height, minHeight, maxHeight);
        }
    }

}
