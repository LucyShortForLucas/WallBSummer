using UnityEngine;

public class WaterView : MonoBehaviour
{
    [SerializeField] private string waterLayerName = "WaterView";

    private int originalLayer;
    private int waterViewLayer;

    private void Awake()
    {
        originalLayer = gameObject.layer;
        waterViewLayer = LayerMask.NameToLayer(waterLayerName);
    }

    private void OnEnable()
    {
        PlayerViews.OnViewToggled += HandleViewChanged;
    }

    private void OnDisable()
    {
        PlayerViews.OnViewToggled -= HandleViewChanged;
    }

    private void Start()
    {
        var playerViews = FindAnyObjectByType<PlayerViews>();
        if (playerViews != null && waterViewLayer != -1)
        {
            gameObject.layer = waterViewLayer;
        }
    }

    private void HandleViewChanged(bool isWaterViewActive)
    {
        if (waterViewLayer != -1)
        {
            gameObject.layer = isWaterViewActive ? waterViewLayer : originalLayer;
        }
    }
}