using UnityEngine;

public class FarmingAudioEvents : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event _plantingEvent;
    private FarmPlot _farmPlotReference;
    private void Awake()
    {
        _farmPlotReference = GetComponent<FarmPlot>();
    }
    private void OnEnable()
    {
        _farmPlotReference.OnPlantEvent += PlantEvent;
    }
    private void OnDisable()
    {
        _farmPlotReference.OnPlantEvent -= PlantEvent;
    }

    private void PlantEvent()
    {
        _plantingEvent.Post(gameObject);
    }
}
