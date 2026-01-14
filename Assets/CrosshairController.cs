using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject crosshairRoot;     // the Image object (or whole canvas)
    [SerializeField] private CameraController cameraController; // your camera toggle script
    [SerializeField] private FirstPersonController firstPersonController; // to hide on inventory

    private void Awake()
    {
        if (crosshairRoot == null) crosshairRoot = gameObject;

        if (cameraController == null)
            cameraController = Object.FindFirstObjectByType<CameraController>();

        if (firstPersonController == null)
            firstPersonController = Object.FindFirstObjectByType<FirstPersonController>();
    }

    private void Update()
    {
        // Default: show in first person only
        bool isThirdPerson = cameraController != null && cameraController.IsThirdPerson;

        bool inventoryOpen = firstPersonController != null
                             && firstPersonController.InventoryPanel != null
                             && firstPersonController.InventoryPanel.activeSelf;

        // Show only if: first person AND inventory closed
        bool shouldShow = !isThirdPerson && !inventoryOpen;

        if (crosshairRoot.activeSelf != shouldShow)
            crosshairRoot.SetActive(shouldShow);
    }
}
