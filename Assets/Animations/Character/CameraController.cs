using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float sensX = 200f;
    public float sensY = 200f;
    public Transform orientation;
    public Transform followTarget;
    private float yRotation;

    public Transform playerCam;
    public GameObject FPCamera;
    public GameObject TPCamera;
    private bool isThirdPerson = false;
    public bool IsThirdPerson => isThirdPerson;
    public Key toggleKey = Key.Q;
    public Key CameraLockKey = Key.LeftAlt;

    private float prevXInput;
    private float prevYInput;
    public bool cameraLock = true;

    public Volume fpVolume;
    public Volume tpVolume;
    public Volume sandVolume;
    public Volume snowVolume;
    public Volume grassVolume;

    public ParticleSystem sandFX;
    public ParticleSystem snowFX;
    public ParticleSystem grassFX;


    public string biome = "";
    public float transitionSpeed = 3f;
    public float targetFXRate = 300f;

    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    private CinemachinePOV pov;

    // --- NEW: Persistent & scene load support ---
    private void Awake()
    {
        // Prevent duplicates
        if (UnityEngine.Object.FindObjectsByType<CameraController>(UnityEngine.FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // Keep camera across dungeon scenes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignPlayerIfExists();
    }

    private void AssignPlayerIfExists()
    {
        if (followTarget == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                followTarget = playerObj.transform;
            }
        }
    }
    // --- END NEW ---

    private void Start()
    {
        AssignPlayerIfExists();

        if (freeLookCamera == null)
            freeLookCamera = GetComponent<CinemachineFreeLook>();

        if (virtualCam == null)
            virtualCam = GetComponent<CinemachineVirtualCamera>();

        pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();

        SetSensitivity(sensX, sensY);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetSensitivity(float horizontal, float vertical)
    {
        freeLookCamera.m_XAxis.m_MaxSpeed = horizontal;
        freeLookCamera.m_YAxis.m_MaxSpeed = vertical / 100;
        pov.m_HorizontalAxis.m_MaxSpeed = horizontal;
        pov.m_VerticalAxis.m_MaxSpeed = vertical / 2;
    }

    private void Update()
    {
        // Cursor lock
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Camera toggle
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame) ToggleView();
        if (Keyboard.current != null && Keyboard.current[CameraLockKey].wasPressedThisFrame && isThirdPerson) cameraLock = !cameraLock;

        string prevBiome = biome;
        biome = CheckBiome();
        TransitionBiome(biome);
    }

    private void LateUpdate()
    {
        if (followTarget == null)
        {
            AssignPlayerIfExists();
            if (followTarget == null) return;
        }

        // Only rotate player to match camera in FP mode
        if (!isThirdPerson && playerCam != null)
        {
            Vector3 cameraYaw = new Vector3(0f, playerCam.eulerAngles.y, 0f);
            followTarget.rotation = Quaternion.Euler(cameraYaw);
        }
    }

    // --- Biome / terrain functions remain unchanged ---
    private string CheckBiome()
    {
        Ray ray = new Ray(followTarget.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            if (hit.collider.name == "Terrain Chunk")
            {
                const float scale = 5f;
                int chunkSize = mapGenerator.mapChunkSize - 1;

                float newHitX = hit.point.x + (0.5f * (chunkSize * scale));
                float newHitZ = hit.point.z + (0.5f * (chunkSize * scale));
                Vector2 hitScaled = new Vector2(newHitX, newHitZ) / scale;

                Vector2Int chunkCoord = new Vector2Int(
                    Mathf.FloorToInt(hitScaled.x / chunkSize),
                    Mathf.FloorToInt(hitScaled.y / chunkSize)
                );

                if (!EndlessTerrain.Instance.terrainChunkDictionary.TryGetValue(chunkCoord, out EndlessTerrain.TerrainChunk terrainChunk))
                {
                    Debug.LogWarning($"No chunk found for {chunkCoord}");
                    return "ERROR";
                }

                int size = terrainChunk.mapData.chunkSize;
                Transform chunkRoot = hit.collider.transform;

                int localX = Mathf.FloorToInt((hit.point.x - (chunkRoot.position.x - (chunkSize * scale * 0.5f))) / scale);
                int localY = chunkSize - Mathf.FloorToInt((hit.point.z - (chunkRoot.position.z - (chunkSize * scale * 0.5f))) / scale);

                localX = Mathf.Clamp(localX, 0, size - 1);
                localY = Mathf.Clamp(localY, 0, size - 1);

                BiomeCoord biomeCoord = terrainChunk.mapData.biomeGenData.voronoiMap[localX, localY];

                Debug.DrawLine(chunkRoot.position, hit.point, Color.red, 0.1f);

                return biomeCoord.getBiome();
            }
        }
        return "";
    }

    void TransitionBiome(string biome)
    {
        var sandEmission = sandFX.emission;
        var snowEmission = snowFX.emission;
        var grassEmission = grassFX.emission;

        if (biome == "Desert")
        {
            sandVolume.weight = Mathf.Lerp(sandVolume.weight, 1, transitionSpeed * Time.deltaTime);
            snowVolume.weight = Mathf.Lerp(snowVolume.weight, 0, transitionSpeed * Time.deltaTime);
            grassVolume.weight = Mathf.Lerp(grassVolume.weight, 0, transitionSpeed * Time.deltaTime);

            sandEmission.rateOverTime = Mathf.Lerp(sandFX.emission.rateOverTime.constant, (targetFXRate / 20), transitionSpeed * Time.deltaTime);
            snowEmission.rateOverTime = Mathf.Lerp(snowFX.emission.rateOverTime.constant, 0f, transitionSpeed * Time.deltaTime);
            grassEmission.rateOverTime = Mathf.Lerp(grassFX.emission.rateOverTime.constant, 0f, transitionSpeed * Time.deltaTime);
        }
        else if (biome == "Snow")
        {
            sandVolume.weight = Mathf.Lerp(sandVolume.weight, 0, transitionSpeed * Time.deltaTime);
            snowVolume.weight = Mathf.Lerp(snowVolume.weight, 1, transitionSpeed * Time.deltaTime);
            grassVolume.weight = Mathf.Lerp(grassVolume.weight, 0, transitionSpeed * Time.deltaTime);

            sandEmission.rateOverTime = Mathf.Lerp(sandFX.emission.rateOverTime.constant, 0f, transitionSpeed * Time.deltaTime);
            snowEmission.rateOverTime = Mathf.Lerp(snowFX.emission.rateOverTime.constant, targetFXRate, transitionSpeed * Time.deltaTime);
            grassEmission.rateOverTime = Mathf.Lerp(grassFX.emission.rateOverTime.constant, 0f, transitionSpeed * Time.deltaTime);
        }
        else if (biome == "Grass Plains")
        {
            sandVolume.weight = Mathf.Lerp(sandVolume.weight, 0, transitionSpeed * Time.deltaTime);
            snowVolume.weight = Mathf.Lerp(snowVolume.weight, 0, transitionSpeed * Time.deltaTime);
            grassVolume.weight = Mathf.Lerp(grassVolume.weight, 1, transitionSpeed * Time.deltaTime);

            sandEmission.rateOverTime = Mathf.Lerp(sandFX.emission.rateOverTime.constant, 0f, transitionSpeed * Time.deltaTime);
            snowEmission.rateOverTime = Mathf.Lerp(snowFX.emission.rateOverTime.constant, 0f, transitionSpeed * Time.deltaTime);
            grassEmission.rateOverTime = Mathf.Lerp(grassFX.emission.rateOverTime.constant, (targetFXRate / 20), transitionSpeed * Time.deltaTime);
        }
    }

    void ToggleView()
    {
        isThirdPerson = !isThirdPerson;

        if (isThirdPerson)
        {
            virtualCam.Priority = 10;
            freeLookCamera.Priority = 20;
            tpVolume.weight = 1;
            fpVolume.weight = 0;
        }
        else
        {
            virtualCam.Priority = 20;
            freeLookCamera.Priority = 10;
            cameraLock = true;
            tpVolume.weight = 0;
            fpVolume.weight = 1;
        }
    }
}
