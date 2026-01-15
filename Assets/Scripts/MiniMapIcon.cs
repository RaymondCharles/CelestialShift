using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    [Header("References")]
    public Camera mapCamera;
    public RectTransform mapPanel;
    public GameObject defaultIconPrefab;

    private List<ItemInstance> worldItems = new List<ItemInstance>();
    private Dictionary<ItemInstance, GameObject> itemIcons = new Dictionary<ItemInstance, GameObject>();

    void Start()
    {

        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject go in items)
        {
            ItemInstance instance = go.GetComponent<ItemInstance>();
            if (instance == null) continue;

            worldItems.Add(instance);


            GameObject prefabToUse = instance.minimapIconPrefab != null ? instance.minimapIconPrefab : defaultIconPrefab;

            GameObject icon = Instantiate(prefabToUse, mapPanel);
            itemIcons[instance] = icon;
        }
    }

    void Update()
    {
        foreach (ItemInstance item in worldItems)
        {
            if (item == null) continue;

            GameObject icon = itemIcons[item];
            RectTransform iconRect = icon.GetComponent<RectTransform>();

            Vector3 viewportPos = mapCamera.WorldToViewportPoint(item.transform.position);
            bool onMap = viewportPos.z > 0 &&
                         viewportPos.x >= 0f && viewportPos.x <= 1f &&
                         viewportPos.y >= 0f && viewportPos.y <= 1f;

            if (onMap)
            {
                float x = (viewportPos.x - 0.5f) * mapPanel.rect.width;
                float y = (viewportPos.y - 0.5f) * mapPanel.rect.height;

                iconRect.anchoredPosition = new Vector2(x, y);
                icon.SetActive(true);
            }
            else
            {
                icon.SetActive(false);
            }
        }
    }
}
    
