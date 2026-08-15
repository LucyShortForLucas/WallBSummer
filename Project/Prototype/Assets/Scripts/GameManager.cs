using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int currentDay = 1;

    [Header("Time")]
    public float secondsPerDay = 60f;
    private float dayTimer = 0f;

    [Header("Lighting")]
    public Light sunLight;
    public float dayIntensity = 1f;
    public float nightIntensity = 0.1f;

    public event Action<int> OnDayAdvanced;

    private static CentralResourceHub _resourceHub;

    public static CentralResourceHub ResourceHub
    {
        get
        {
            if (_resourceHub == null)
            {
                _resourceHub = new CentralResourceHub();
            }
            return _resourceHub;
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (_resourceHub == null)
        {
            _resourceHub = new CentralResourceHub();
        }
    }

    private void Update()
    {
        dayTimer += Time.deltaTime;

        if (sunLight != null)
        {
            float timePercent = dayTimer / secondsPerDay;

            float lightMultiplier = Mathf.Sin(timePercent * Mathf.PI);

            sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, lightMultiplier);
        }

        if (dayTimer >= secondsPerDay)
        {
            dayTimer -= secondsPerDay;
            AdvanceDay();
        }
    }

    public void AdvanceDay()
    {
        currentDay++;
        Debug.Log("Day " + currentDay + " has begun.");

        OnDayAdvanced?.Invoke(currentDay);
    }
}