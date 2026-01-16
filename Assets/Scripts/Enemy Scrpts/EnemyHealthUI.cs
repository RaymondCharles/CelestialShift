using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    // References
    
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Slider slider;

    // Status
    [SerializeField] private EnemyStatusEffects status;
    [SerializeField] private GameObject freezeIcon;
    [SerializeField] private GameObject poisonIcon;
    [SerializeField] private GameObject burnIcon;

    // Camera
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Camera targetCamera;

    // Levels
    [SerializeField] private EnemyLevel enemyLevel;
    [SerializeField] private TMP_Text levelText;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<EnemyHealth>();

        if (status == null)
            status = GetComponentInParent<EnemyStatusEffects>();

        if (enemyLevel == null)
            enemyLevel = GetComponentInParent<EnemyLevel>();

        if (levelText == null)
            levelText = GetComponentInChildren<TMP_Text>(true);
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

        if (levelText != null)
        {
            int lvl = (enemyLevel != null) ? enemyLevel.level : 1;
            levelText.text = $"Lv. {lvl}";
        }


        if (targetCamera == null)
            targetCamera = Camera.main;

        // start hidden (optional safety)
        if (freezeIcon != null) freezeIcon.SetActive(false);
        if (poisonIcon != null) poisonIcon.SetActive(false);
        if (burnIcon != null) burnIcon.SetActive(false);
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
            if (burnIcon != null) burnIcon.SetActive(status.IsBurning);
        }
        else
        {
            if (freezeIcon != null) freezeIcon.SetActive(false);
            if (poisonIcon != null) poisonIcon.SetActive(false);
            if (burnIcon != null) burnIcon.SetActive(false);
        }

        if (levelText != null)
        {
            int lvl = (enemyLevel != null) ? enemyLevel.level : 1;
            levelText.text = $"Lv. {lvl}";
        }


        // Face camera
        if (faceCamera && targetCamera != null)
        {
            Vector3 dir = transform.position - targetCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
