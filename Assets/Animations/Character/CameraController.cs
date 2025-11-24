using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

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
    public Key toggleKey = Key.Q; //Keybind for switching between cameras
    public Key enemyLock = Key.F; //Keybind for locking in on an enemy

    
    private float prevXInput;
    private float prevYInput;

    


    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    private CinemachinePOV pov;

    private void Start()
    {
        if (freeLookCamera == null)
            freeLookCamera = GetComponent<CinemachineFreeLook>();

        if (virtualCam == null)
            virtualCam = GetComponent<CinemachineVirtualCamera>();

        pov = virtualCam.GetCinemachineComponent<CinemachinePOV>();

        SetSensitivity(sensX, sensY);

        // Lock cursor on start for FPV movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //Changes cinemachine sensitivity
    public void SetSensitivity(float horizontal, float vertical)
    {
        freeLookCamera.m_XAxis.m_MaxSpeed = horizontal; // horizontal rotation speed
        freeLookCamera.m_YAxis.m_MaxSpeed = vertical / 100;   // vertical rotation speed
        pov.m_HorizontalAxis.m_MaxSpeed = horizontal;
        pov.m_VerticalAxis.m_MaxSpeed = vertical / 2;
    }

    // Update is called once per frame
    void Update()
    {

        
        // 1. Cursor Management for Resuming Control (Click to lock)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 2. Toggling between camera views (Press q to swap)
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleView();
        }
    }

    void LateUpdate()
    {
        Vector3 yawOnly = new Vector3(0f, playerCam.eulerAngles.y, 0f);
        orientation.rotation = Quaternion.Euler(yawOnly);
        followTarget.rotation = Quaternion.Euler(yawOnly);

    }


    // Public method called by the UI button (or Q key)
    void ToggleView()
    {
        isThirdPerson = !isThirdPerson;

        if (isThirdPerson)
        {
            virtualCam.Priority = 10;
            freeLookCamera.Priority = 20;
        }
        else
        {
            virtualCam.Priority = 20;
            freeLookCamera.Priority = 10;
        }
    }

}