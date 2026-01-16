using System;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-115)] // Before input system
public class DynamicNavMeshUpdater : MonoBehaviour {
    [Header("Agent Tracking")] [SerializeField, Tooltip("The agent whose position will be used to update the NavMesh")]
    NavMeshAgent trackedAgent;

    [SerializeField, Range(0.01f, 1f), Tooltip("Quantization factor for position updates (lower = more frequent updates)")]
    float quantizationFactor = 0.1f;

    NavMeshSurface surface;
    Vector3 volumeSize;

    void Awake() {
        surface = GetComponent<NavMeshSurface>();
    }

    void OnEnable() {
        volumeSize = surface.size;
        surface.center = GetQuantizedCenter();
        surface.BuildNavMesh();
    }

    void Update() {
        var updatedCenter = GetQuantizedCenter();
        var updateNavMesh = false;

        if (surface.center != updatedCenter) {
            surface.center = updatedCenter;
            updateNavMesh = true;
        }

        if (surface.size != volumeSize) {
            volumeSize = surface.size;
            updateNavMesh = true;
        }

        if (updateNavMesh) {
            surface.UpdateNavMesh(surface.navMeshData);
        }
    }

    Vector3 GetQuantizedCenter() {
        Debug.Log(trackedAgent.transform.position.Quantize(quantizationFactor * surface.size));
        return trackedAgent.transform.position.Quantize(quantizationFactor * surface.size);
    }

    void OnDrawGizmosSelected() {
        if (surface == null) surface = GetComponent<NavMeshSurface>();
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawCube(transform.position + surface.center, surface.size);
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawWireCube(transform.position + surface.center, surface.size);
    }
}

public static class Vector3Extensions {
    public static Vector3 Quantize(this Vector3 position, Vector3 quantization) {
        return Vector3.Scale(
            quantization,
            new Vector3(
                Mathf.Floor(position.x / quantization.x),
                Mathf.Floor(position.y / quantization.y),
                Mathf.Floor(position.z / quantization.z)
            ));
    }
}