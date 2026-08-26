using UnityEngine;
using TMPro;

public class WaterUsagePanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text waterAmountText;

    private InventoryUIManager uiManager;

    public void Refresh(InventoryUIManager manager)
    {
        uiManager = manager;
    }

    private void Update()
    {
        if (uiManager == null || uiManager.CurrentFactory == null) return;

        WaterComponent waterComp = uiManager.CurrentFactory.WaterComponent;

        if (waterComp.WaterCapacity > 0)
        {
            int percent = Mathf.RoundToInt((waterComp.CurrentWater / waterComp.WaterCapacity) * 100f);
            waterAmountText.text = $"Water: {percent}%";
        }
        else
        {
            waterAmountText.text = "Water: 0%";
        }
    }
}