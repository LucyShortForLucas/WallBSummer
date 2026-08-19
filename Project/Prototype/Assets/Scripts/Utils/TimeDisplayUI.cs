using UnityEngine;
using TMPro;

public class TimeDisplayUI : MonoBehaviour, IInjectable
{
    [Header("UI References")]
    [SerializeField] private TMP_Text timeText;

    private TimeManager timeManager;

    public void Inject(DependencyContainer container)
    {
        timeManager = container.Get<TimeManager>();
    }

    private void Update()
    {
        if (timeManager == null || timeText == null) return;

        float currentHourFloat = timeManager.TimePercent * 24f;

        int currentHour = Mathf.FloorToInt(currentHourFloat);

        timeText.text = $"{currentHour} Hour";
    }
}