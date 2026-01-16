using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AsyncNavMeshBuilder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private NavMeshSurface surface;

    [Header("Build Area")]
    [SerializeField] private Vector3 buildSize = new Vector3(300, 80, 300);
    [SerializeField] private float rebuildDistance = 100f;

    [Header("Rebuild Control")]
    [Tooltip("If true, chunk code can call RequestRebuild() and we'll rebuild once when ready.")]
    [SerializeField] private bool allowRequestedRebuilds = true;

    private NavMeshData navMeshData;
    private Vector3 lastBuildCenter;
    private AsyncOperation currentBuild;

    private readonly List<NavMeshBuildSource> sources = new();
    private static readonly List<NavMeshBuildMarkup> markups = new(); // keep one list (no allocations)

    private bool pendingRebuild;

    public static AsyncNavMeshBuilder Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("AsyncNavMeshBuilder: Player reference is missing.");
            enabled = false;
            return;
        }

        if (surface == null)
        {
            Debug.LogError("AsyncNavMeshBuilder: NavMeshSurface reference is missing.");
            enabled = false;
            return;
        }

        // Create and register runtime navmesh data
        navMeshData = new NavMeshData(surface.agentTypeID);
        NavMesh.AddNavMeshData(navMeshData);

        lastBuildCenter = player.position;
        pendingRebuild = true; // build once at start
    }

    /// <summary>
    /// Call this from chunk code after you update MeshCollider/sharedMesh.
    /// It will rebuild once, safely (debounced).
    /// </summary>
    public void RequestRebuild()
    {
        if (!allowRequestedRebuilds) return;
        pendingRebuild = true;
    }

    private void Update()
    {
        if (player == null || surface == null) return;

        // Only start a new build if the previous one finished
        bool buildIdle = (currentBuild == null) || currentBuild.isDone;

        // Trigger rebuild if player moved far enough
        if (Vector3.Distance(player.position, lastBuildCenter) > rebuildDistance)
            pendingRebuild = true;

        if (pendingRebuild && buildIdle)
        {
            pendingRebuild = false;
            RebuildAsync();
        }

        if (NavMesh.SamplePosition(player.position, out var hit, 5f, NavMesh.AllAreas))
        {
            Debug.Log("NavMesh exists near player");
        }
        else
        {
            Debug.LogWarning("No NavMesh near player");
        }
    }

    private void RebuildAsync()
    {

        lastBuildCenter = player.position;
        var bounds = new Bounds(lastBuildCenter, buildSize);

        sources.Clear();
        NavMeshBuilder.CollectSources(
            bounds,
            surface.layerMask,
            surface.useGeometry,
            surface.defaultArea,
            markups,
            sources
        );

        currentBuild = NavMeshBuilder.UpdateNavMeshDataAsync(
            navMeshData,
            surface.GetBuildSettings(),
            sources,
            bounds
        );

        Debug.Log($"BUILDING: sources={sources.Count} boundsCenter={bounds.center} size={bounds.size}");
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.matrix = Matrix4x4.TRS(player.position, Quaternion.identity, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, buildSize);
    }
}