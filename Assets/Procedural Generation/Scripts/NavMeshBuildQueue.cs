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
        public bool isQuarter;
        public int quarterIndex;
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

    public void EnqueueFull(NavMeshSurface surface, EndlessTerrain.TerrainChunk chunk, float overlapWorld = 5f)
    {
        queue.Enqueue(new Request { surface = surface, chunk = chunk, overlap = overlapWorld, isQuarter = false });
        if (!building) StartCoroutine(Process());
    }

    public void EnqueueQuarter(NavMeshSurface surface, EndlessTerrain.TerrainChunk chunk, int quarter, float overlapWorld = 5f)
    {
        queue.Enqueue(new Request { surface = surface, chunk = chunk, overlap = overlapWorld, isQuarter = true, quarterIndex = quarter });
        if (!building) StartCoroutine(Process());
    }







    private IEnumerator Process()
    {
        building = true;

        while (queue.Count > 0)
        {
            Request r = queue.Dequeue();
            if (r.surface == null || r.chunk == null) continue;
            if (r.chunk.navMeshData == null) continue; // chunk must have created it

            Bounds bounds;
            NavMeshData data;

            if (!r.isQuarter)
            {
                bounds = r.chunk.GetNavBuildBounds(r.overlap);      // your full-chunk bounds method
                data = r.chunk.navMeshData;                         // low/full data
            }
            else
            {
                bounds = r.chunk.GetQuarterBounds(r.quarterIndex, r.overlap);
                data = r.chunk.hiNavData[r.quarterIndex];           // hi quarter data
            }

            

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
                data,
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






/*
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
}*/