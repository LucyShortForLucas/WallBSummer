using UnityEngine;
using UnityEngine.UI;

public class LaunchPanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button launchButton;

    private InventoryUIManager uiManager;
    private bool isInitialized = false;

    public void Refresh(InventoryUIManager manager)
    {
        uiManager = manager;

        if (!isInitialized)
        {
            // Subscribe
            if (launchButton != null)
            {
                launchButton.onClick.AddListener(OnLaunchClicked);
            }
            isInitialized = true;
        }
    }

    private void OnLaunchClicked()
    {
        // Trigger
        if (uiManager != null && uiManager.CurrentDropBuilding != null)
        {
            uiManager.CurrentDropBuilding.TriggerLaunch();
        }
    }

    private void Update()
    {
        if (uiManager == null || uiManager.CurrentDropBuilding == null || launchButton == null) return;

        // Update launch button state
        if (uiManager.CurrentDropBuilding.IsWaiting)
        {
            launchButton.interactable = false;
        }
        else
        {
            launchButton.interactable = uiManager.CurrentDropBuilding.CanLaunch;
        }
    }
}