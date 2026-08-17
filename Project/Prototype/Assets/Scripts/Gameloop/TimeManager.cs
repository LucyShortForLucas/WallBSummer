using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("Game State")]
    public int currentDay = 1;

    [Header("Time")]
    [SerializeField] private float secondsPerDay = 60f;
    private float dayTimer = 0f;

    [Header("Lighting")]
    [SerializeField] private Light sunLight;
    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private float nightIntensity = 0.1f;

    public event Action<int> OnDayAdvanced;

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