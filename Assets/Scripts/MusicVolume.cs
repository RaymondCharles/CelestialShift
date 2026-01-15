using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MusicVolume : MonoBehaviour
{
    public Slider volumeslider;

    public void Start()
        {
            float savedVolume = PlayerPrefs.HasKey("MusicVol") ? PlayerPrefs.GetFloat("MusicVol") : 0.6f;
            volumeslider.value = savedVolume;

            ApplyVolume(savedVolume);
            volumeslider.onValueChanged.AddListener(ApplyVolume);


         }
         public void ApplyVolume(float value)
            {
                if (GameManager.Instance != null && GameManager.Instance.currentMusic != null)
                    {
                        GameManager.Instance.currentMusic.volume = value;
                    }
                PlayerPrefs.SetFloat("MusicVol", value);

            }
}
