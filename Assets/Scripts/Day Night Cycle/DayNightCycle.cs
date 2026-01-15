using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DayNightCycle : MonoBehaviour {
    
    [Header("Time")]
    [Tooltip("Day Length in Minutes")]
    [SerializeField]
    private float _targetDayLength = 0.5f; //length of day in minutes

    public float targetDayLength
    {
        get
        {
            return _targetDayLength;
        }
    }

    [SerializeField]
    private float elapsedTime;

    [SerializeField]
    private bool use24Clock = true;

    [SerializeField]
    private Text clockText;

    [SerializeField]
    [Range(0f, 1f)]
    private float _timeOfDay;
    public float timeOfDay
    {
        get
        {
            return _timeOfDay;
        }
    }

    [SerializeField]
    private int _dayNumber = 0; //tracks the days passed
    public int dayNumber
    {
        get
        {
            return _dayNumber;
        }
    }
    
    [SerializeField]
    private int _yearNumber = 0; // tracks the years passed
    public int yearNumber
    {
        get
        {
            return _yearNumber;
        }
    }
    private float _timeScale = 100f; // how fast time moves in the game world
    
    [SerializeField]
    private int _yearLength = 100; // number of days in a year
    public float yearLength
    {
        get
        {
            return _yearLength;
        }
    }

    [SerializeField]
    public bool pause = false; // pauses the day night cycle when true for debugging
    
    [SerializeField]
    private AnimationCurve timeCurve;
    private float timeCurveNormalization;


    [Header("Sun Light")]
    [SerializeField]
    private Transform dailyRotation;
    [SerializeField]
    private Light sun;
    private float intensity;
    [SerializeField]
    private float sunBaseIntensity = 1f;
    [SerializeField]
    private float sunVariation = 1.5f;
    [SerializeField]
    private Gradient sunColor;


    [Header("Seasonal Variables")]
    [SerializeField]
    private Transform sunSeasonalRotation;
    [SerializeField]
    [Range(-45f, 45f)]
    private float maxSeasonalTilt;


    private void Start()
    {
        NormalTimeCurve(); 
    }


    private void Update()
    {
        if (dailyRotation == null)
        {
            Debug.LogError("DayNightCycle: dailyRotation is NOT assigned!");
            return;
        }
        if (sun == null)
        {
            Debug.LogError("DayNightCycle: sun is NOT assigned!");
            return;
        }
        if (!pause)
        {
            UpdateTimeScale();
            UpdateTime();
            //UpdateClock();
        }


        AdjustSunRotation();
        SunIntensity();
        AdjustSunColor();
    }


    private void UpdateTimeScale()
    {
        _timeScale = 24 / (_targetDayLength / 60); // base time scale calculation - 24 / (day length in hours)
        //_timeScale *= timeCurve.Evaluate(elapsedTime / (targetDayLength * 60)); //changes timescale based on time curve
        //_timeScale /= timeCurveNormalization; //keeps day length at target value
    }


    private void NormalTimeCurve()
    {
        float stepSize = 0.01f;
        int numberSteps = Mathf.FloorToInt(1f / stepSize);
        float curveTotal = 0;


        for (int i = 0; i < numberSteps; i++)
        {
            curveTotal += timeCurve.Evaluate(i * stepSize);
        }


        timeCurveNormalization = curveTotal / numberSteps; //keeps day length at target value
    }


    private void UpdateTime()
    {
        _timeOfDay += Time.deltaTime * _timeScale / 86400; // seconds in a day
        elapsedTime += Time.deltaTime;
        if(_timeOfDay > 1) //new day
        {
            elapsedTime = 0;
            _dayNumber++;
            _timeOfDay -= 1;


            if(_dayNumber > _yearLength) //new year
            {
                _yearNumber++;
                _dayNumber = 0;
            }
        }
    }


    private void UpdateClock()
    {
        float time = elapsedTime / (targetDayLength * 60);
        float hour = Mathf.FloorToInt(time * 24);
        float minute = Mathf.FloorToInt(((time * 24) - hour) * 60);


        string hourString;
        string minuteString;


        if (!use24Clock && hour > 12)
            hour -= 12;


        if (hour < 10)
            hourString = "0" + hour.ToString();
        else
            hourString = hour.ToString();


        if (minute < 10)
            minuteString = "0" + minute.ToString();
        else
            minuteString = minute.ToString();
        
        if(use24Clock)
            clockText.text = hourString + " : " + minuteString;
        else if (time > 0.5f)
            clockText.text = hourString + " : " + minuteString + " pm";
        else
            clockText.text = hourString + " : " + minuteString + " am";

    }


    //rotates the sun daily (and seasonally soon too);
    private void AdjustSunRotation()
    {
        float sunAngle = timeOfDay * 360f; // calculate sun angle based on time of day
        dailyRotation.transform.localRotation = Quaternion.Euler(new Vector3(0f, sunAngle, 0f));
    }


    private void SunIntensity()
    {
        // calculate sun intensity based on its angle
        intensity = Vector3.Dot(sun.transform.forward, Vector3.down);
        intensity = Mathf.Clamp01(intensity);

        sun.intensity = intensity * sunVariation + sunBaseIntensity;
    }


    private void AdjustSunColor()
    {
        // adjust sun colour according to gradient based on intensity
        sun.color = sunColor.Evaluate(intensity);
    }

}

