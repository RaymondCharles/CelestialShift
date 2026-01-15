using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChunkNavMeshManager : MonoBehaviour
{
    public static ChunkNavMeshManager Instance { get; private set; }

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface surface; // from NavMeshComponents repo
    [SerializeField] private int maxConcurrentBuilds = 1;

    private readonly Queue<System.Action> buildQueue = new();
    private int buildsInFlight;

    private void Awake()
    {
        Instance = this;
    }

    public NavMeshBuildSettings GetSettings()
    {
        return surface.GetBuildSettings();
    }

    public int AgentTypeID => surface.agentTypeID;

    // Enqueue builds so you don't spike by building many chunks same frame
    public void EnqueueBuild(System.Action buildAction)
    {
        buildQueue.Enqueue(buildAction);
    }

    private void Update()
    {
        if (buildsInFlight >= maxConcurrentBuilds) return;
        if (buildQueue.Count == 0) return;

        buildsInFlight++;
        buildQueue.Dequeue()?.Invoke();
    }

    // Call this from chunks when a build op finishes
    public void NotifyBuildFinished()
    {
        buildsInFlight = Mathf.Max(0, buildsInFlight - 1);
    }
}