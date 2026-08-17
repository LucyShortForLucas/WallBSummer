using System;
using System.Collections.Generic;
using UnityEngine;

public enum StoragePanelType
{
    BigInOut,
    SmallInOut,
    Out,
    Launch
}

public class InventoryUIManager : MonoBehaviour, IInjectable
{
    [Header("Data")]
    [SerializeField] private GlobalResourceDatabase resourceDatabase;

    [Serializable]
    public struct PanelConfig
    {
        public StoragePanelType panelType;
        public GameObject panelRoot;
    }

    [Header("Panel Mappings")]
    [SerializeField] private List<PanelConfig> panels = new List<PanelConfig>();

    private int currentTargetId = -1;
    private int currentPlayerId = -1;
    private List<StoragePanelType> activePanelTypes = new List<StoragePanelType>();

    private CentralResourceHub resourceHub;

    // Getters and Setters
    public GlobalResourceDatabase ResourceDatabase { get => resourceDatabase; set => resourceDatabase = value; }

    public void Inject(DependencyContainer container)
    {
        resourceHub = container.Get<CentralResourceHub>();
        resourceHub.OnResourceChanged += HandleResourceChanged;
    }

    private void Awake()
    {
        if (resourceDatabase != null)
        {
            resourceDatabase.Initialize();
        }

        CloseAllPanels();
    }

    private void OnDestroy()
    {
        if (resourceHub != null)
        {
            resourceHub.OnResourceChanged -= HandleResourceChanged;
        }
    }

    public void OpenUI(int targetStorageId, int playerStorageId, List<StoragePanelType> panelTypes)
    {
        currentTargetId = targetStorageId;
        currentPlayerId = playerStorageId;
        activePanelTypes = new List<StoragePanelType>(panelTypes);

        CloseAllPanels();
        // Turn on only requested panels
        foreach (var pType in activePanelTypes)
        {
            foreach (var config in panels)
            {
                if (config.panelType == pType && config.panelRoot != null)
                {
                    config.panelRoot.SetActive(true);
                    break;
                }
            }
        }

        // Show the latest data
        RefreshUI();
    }

    public void CloseUI()
    {
        // Hide everything and reset our tracking variables
        CloseAllPanels();
        currentTargetId = -1;
        activePanelTypes.Clear();
    }

    private void CloseAllPanels()
    {
        // Go through every panel known, turn off and clear 
        foreach (var config in panels)
        {
            if (config.panelRoot != null)
            {
                config.panelRoot.SetActive(false);
                var panelUI = config.panelRoot.GetComponent<InventoryPanelUI>();
                if (panelUI != null) panelUI.Clear();
            }
        }
    }

    private void HandleResourceChanged(int storageId, int resourceId, int newAmount, int maxAmount)
    {
        if (currentTargetId != -1 && (storageId == currentTargetId || storageId == currentPlayerId))
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        // Every visible panel grab latest info and redraw
        foreach (var config in panels)
        {
            if (config.panelRoot != null && config.panelRoot.activeSelf)
            {
                var panelUI = config.panelRoot.GetComponent<InventoryPanelUI>();
                if (panelUI != null)
                {
                    panelUI.Refresh(currentTargetId, currentPlayerId, resourceHub, resourceDatabase);
                }
            }
        }
    }
}