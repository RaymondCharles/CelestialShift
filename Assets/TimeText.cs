using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimeText : MonoBehaviour
{
    public TMP_Text clockText;
    // Update is called once per frame
    void Update()
    {
        clockText.text = DayNightCycle.Instance.UpdateClock();
    }
}
