using UnityEngine;
using Cinemachine;

public class FreeLookClampY : MonoBehaviour
{
    public CinemachineFreeLook freeLook;
    [Range(0f, 1f)] public float minY = 0.05f;
    [Range(0f, 1f)] public float maxY = 0.9f;

    void LateUpdate()
    {
        if (!freeLook) return;
        freeLook.m_YAxis.Value = Mathf.Clamp(freeLook.m_YAxis.Value, minY, maxY);
    }
}