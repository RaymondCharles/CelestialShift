using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Slider slider;

    [Header("Status (optional)")]
    [SerializeField] private EnemyStatusEffects status;
    [SerializeField] private GameObject freezeIcon;
    [SerializeField] private GameObject poisonIcon;

    [Header("Billboard")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();

        if (status == null)
            status = GetComponentInParent<EnemyStatusEffects>();
    }

    private void Start()
    {
        if (enemyHealth == null || slider == null)
        {
            Debug.LogError($"EnemyHealthUI missing refs on {name}. enemyHealth={enemyHealth}, slider={slider}");
            enabled = false;
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = enemyHealth.MaxHealth;
        slider.value = enemyHealth.CurrentHealth;
        slider.wholeNumbers = false;

        if (targetCamera == null)
            targetCamera = Camera.main;

        // start hidden (optional safety)
        if (freezeIcon != null) freezeIcon.SetActive(false);
        if (poisonIcon != null) poisonIcon.SetActive(false);
    }

    private void LateUpdate()
    {
        if (enemyHealth == null || slider == null) return;

        slider.maxValue = enemyHealth.MaxHealth;
        slider.value = enemyHealth.CurrentHealth;

        // Show status icons
        if (status != null)
        {
            if (freezeIcon != null) freezeIcon.SetActive(status.IsFrozen);
            if (poisonIcon != null) poisonIcon.SetActive(status.IsPoisoned);
        }
        else
        {
            // If no status component exists, keep icons off
            if (freezeIcon != null) freezeIcon.SetActive(false);
            if (poisonIcon != null) poisonIcon.SetActive(false);
        }

        // Face camera
        if (faceCamera && targetCamera != null)
        {
            Vector3 dir = transform.position - targetCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
