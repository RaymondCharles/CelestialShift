using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBuildQueue : MonoBehaviour
{
    public static NavMeshBuildQueue Instance;
    private readonly Queue<NavMeshSurface> queue = new();
    private readonly List<NavMeshSurface> completedSurfaces = new();

    private void Awake() => Instance = this;

    public void Enqueue(NavMeshSurface surface, EndlessTerrain.TerrainChunk chunk)
    {
        if (surface != null && !completedSurfaces.Contains(surface)) queue.Enqueue(surface);
    }

    private void LateUpdate()
    {
        // Build at most one per frame
        if (queue.Count == 0) return;

        NavMeshSurface s = queue.Dequeue();

        //if (s != null)
        s.BuildNavMesh();
        Debug.Log(queue.Count);
    }
}