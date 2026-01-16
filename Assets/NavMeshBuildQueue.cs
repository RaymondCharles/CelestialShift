using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBuildQueue : MonoBehaviour
{
    public static NavMeshBuildQueue Instance { get; private set; }

    private class Request
    {
        public NavMeshSurface surface;
        public EndlessTerrain.TerrainChunk chunk;
        public float overlap;
    }

    private readonly Queue<Request> queue = new Queue<Request>();
    private bool building;

    private readonly List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>(1024);
    private static readonly List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>(0);

    private void Awake()
    {
        Instance = this;
    }

    public void Enqueue(NavMeshSurface surface, EndlessTerrain.TerrainChunk chunk, float overlapWorld = 5f)
    {
        if (surface == null || chunk == null) return;

        queue.Enqueue(new Request
        {
            surface = surface,
            chunk = chunk,
            overlap = overlapWorld
        });

        if (!building)
            StartCoroutine(Process());
    }

    private IEnumerator Process()
    {
        building = true;

        while (queue.Count > 0)
        {
            Request r = queue.Dequeue();
            if (r.surface == null || r.chunk == null) continue;
            if (r.chunk.navMeshData == null) continue; // chunk must have created it

            // 1) Build bounds (with overlap to remove seams)
            Bounds bounds = r.chunk.GetNavBuildBounds(r.overlap);

            // 2) Collect sources within bounds
            sources.Clear();
            NavMeshBuilder.CollectSources(
                bounds,
                r.surface.layerMask,
                r.surface.useGeometry,
                r.surface.defaultArea,
                markups,
                sources
            );

            // If no sources, skip (prevents pointless bakes)
            if (sources.Count == 0)
                continue;

            // 3) Kick async build
            r.chunk.navBuildOp = NavMeshBuilder.UpdateNavMeshDataAsync(
                r.chunk.navMeshData,
                r.surface.GetBuildSettings(),
                sources,
                bounds
            );

            // 4) Wait for async to finish (does NOT block main thread hard like BuildNavMesh)
            while (r.chunk.navBuildOp != null && !r.chunk.navBuildOp.isDone)
                yield return null;

            // Optional: tiny yield so you don't immediately start another heavy operation same frame
            yield return null;
        }

        building = false;
    }
}



