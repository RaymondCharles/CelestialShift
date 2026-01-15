using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChunkNavmeshAsync : MonoBehaviour
{
    public int agentTypeID = 0;
    public LayerMask includeLayers = ~0;

    public bool IsBuilt { get; private set; }

    private NavMeshData navData;
    private AsyncOperation buildOp;

    public void BuildAsync(Bounds worldBounds)
    {
        IsBuilt = false;

        if (navData == null)
        {
            navData = new NavMeshData(agentTypeID);
            NavMesh.AddNavMeshData(navData, transform.position, transform.rotation);
        }

        var sources = new List<NavMeshBuildSource>();
        var markups = new List<NavMeshBuildMarkup>();

        NavMeshBuilder.CollectSources(
            worldBounds,
            includeLayers,
            NavMeshCollectGeometry.PhysicsColliders,
            0,
            markups,
            sources
        );

        var settings = NavMesh.GetSettingsByID(agentTypeID);

        buildOp = NavMeshBuilder.UpdateNavMeshDataAsync(navData, settings, sources, worldBounds);
        buildOp.completed += _ => IsBuilt = true;
    }

    private void OnDisable()
    {
        if (navData != null)
        {
            NavMesh.RemoveNavMeshData(navData);
            navData = null;
        }
    }
}