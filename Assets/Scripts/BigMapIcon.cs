using System.Collections.Generic;
using UnityEngine;

public class BigMapIcon : MonoBehaviour
{
    [Header("References")]
    public Camera mapCamera;
    public RectTransform mapPanel;
    public Transform dungeonWorldObject;
    public GameObject dungeonIconPrefab;
    public GameObject playerIconPrefab;
    private GameObject dungeonIcon;
    private RectTransform playerIcon;

    void Start()
    {
        dungeonIcon = Instantiate(dungeonIconPrefab, mapPanel);

        if (playerIconPrefab != null)
        {
            GameObject obj = Instantiate(playerIconPrefab, mapPanel);
            playerIcon = obj.GetComponent<RectTransform>();
            playerIcon.anchoredPosition = Vector2.zero;
        }
    }

    void Update()
    {
        if (playerIcon != null)
        {
            playerIcon.anchoredPosition = Vector2.zero;
        }

        if (dungeonWorldObject == null) return;

        Vector3 viewportPos = mapCamera.WorldToViewportPoint(dungeonWorldObject.position);

        bool onMap = viewportPos.z > 0 &&
                     viewportPos.x >= 0f && viewportPos.x <= 1f &&
                     viewportPos.y >= 0f && viewportPos.y <= 1f;

        if (onMap)
        {
            float x = (viewportPos.x - 0.5f) * mapPanel.rect.width;
            float y = (viewportPos.y - 0.5f) * mapPanel.rect.height;

            dungeonIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            dungeonIcon.SetActive(true);
        }
        else
        {
            dungeonIcon.SetActive(false);
        }
    }
}
