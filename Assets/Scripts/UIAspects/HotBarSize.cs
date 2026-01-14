using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HotBarSize : MonoBehaviour
{
    public RectTransform HotBar;
    public Slider sliderSize;
    public float defaultSize = 0.6f;
    // Start is called before the first frame update
    void Start()
    {
        float savedSize = PlayerPrefs.HasKey("HotbARSize") ? PlayerPrefs.GetFloat("HotbARSize") : defaultSize;
        sliderSize.value = savedSize;
        ApplySize(savedSize);

        sliderSize.onValueChanged.AddListener(ApplySize);

    }

    public void ApplySize(float value)
    {

        HotBar.localScale = new Vector3(value, value, value);
        //Inventory.anchoredPosition = Vector2.zero;
        PlayerPrefs.SetFloat("HotbARSize", value);
    }
}
