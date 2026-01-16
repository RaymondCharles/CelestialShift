using UnityEngine;

public class MiniMapCam : MonoBehaviour
{
    public Transform player;  
    public float height = 30.0f;

    void LateUpdate()
    {
       
        if (player == null && FirstPersonController.Instance != null)
        {
            player = FirstPersonController.Instance.transform;
        }

        if (player == null) return; 

        Vector3 newPos = player.position;
        newPos.y = height;
        transform.position = newPos;
    }
}
