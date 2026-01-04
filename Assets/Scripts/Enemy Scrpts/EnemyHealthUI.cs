using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Slider slider;

    [Header("Billboard")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Camera targetCamera;

    private void Awake()
    {
        // Auto-find slider if not set
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        // Auto-find EnemyHealth in parent if not set
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    private void Start()
    {
        if (enemyHealth == null || slider == null)
        {
            Debug.LogError($"EnemyHealthUI missing refs on {name}. enemyHealth={enemyHealth}, slider={slider}");
            enabled = false;
            return;
        }

        // IMPORTANT: set slider range to real health values
        slider.minValue = 0f;
        slider.maxValue = enemyHealth.MaxHealth;
        slider.value = enemyHealth.CurrentHealth;

        // Optional: if you want smooth visuals, keep Whole Numbers off
        slider.wholeNumbers = false;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (enemyHealth == null || slider == null) return;

        // Update value every frame
        slider.maxValue = enemyHealth.MaxHealth;       // in case max changes
        slider.value = enemyHealth.CurrentHealth;

        // Face camera
        if (faceCamera && targetCamera != null)
        {
            Vector3 dir = transform.position - targetCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
