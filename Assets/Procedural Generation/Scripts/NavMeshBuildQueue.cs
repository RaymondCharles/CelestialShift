using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBuildQueue : MonoBehaviour
{
    public static NavMeshBuildQueue Instance;
    private readonly Queue<NavMeshSurface> queue = new();

    private void Awake() => Instance = this;

    public void Enqueue(NavMeshSurface surface)
    {
        if (surface != null) queue.Enqueue(surface);
    }

    private void LateUpdate()
    {
        // Build at most one per frame
        if (queue.Count == 0) return;

        var s = queue.Dequeue();
        if (s != null && s.gameObject.activeInHierarchy)
            s.BuildNavMesh();
    }
}