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

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("Data")]
    public GlobalResourceDatabase resourceDatabase;

    [Serializable]
    public struct PanelConfig
    {
        public StoragePanelType panelType;
        public GameObject panelRoot;
    }

    [Header("Panel Mappings")]
    public List<PanelConfig> panels = new List<PanelConfig>();

    private int currentTargetId = -1;
    private int currentPlayerId = -1;
    private List<StoragePanelType> activePanelTypes = new List<StoragePanelType>();

    private void Awake()
    {
        Instance = this;

        // Ensure resource are loaded
        if (resourceDatabase != null)
        {
            resourceDatabase.Initialize();
        }

        CloseAllPanels();
    }

    private void Start()
    {
        GameManager.ResourceHub.OnResourceChanged += HandleResourceChanged;
    }

    private void OnDestroy()
    {
        if (GameManager.ResourceHub != null)
            GameManager.ResourceHub.OnResourceChanged -= HandleResourceChanged;
    }

    public void OpenUI(int targetStorageId, int playerStorageId, List<StoragePanelType> panelTypes)
    {
        currentTargetId = targetStorageId;
        currentPlayerId = playerStorageId;
        activePanelTypes = new List<StoragePanelType>(panelTypes);

        CloseAllPanels();

        // Enable requested panels
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

        // Show current data
        RefreshUI();
    }

    public void CloseUI()
    {
        CloseAllPanels();
        currentTargetId = -1;
        activePanelTypes.Clear();
    }

    private void CloseAllPanels()
    {
        foreach (var config in panels)
        {
            if (config.panelRoot != null)
            {
                config.panelRoot.SetActive(false);

                // Clean memory
                var panelUI = config.panelRoot.GetComponent<InventoryPanelUI>();
                if (panelUI != null) panelUI.Clear();
            }
        }
    }

    private void HandleResourceChanged(int storageId, int resourceId, int newAmount, int maxAmount)
    {
        // If changed inventory is on screen
        if (currentTargetId != -1 && (storageId == currentTargetId || storageId == currentPlayerId))
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        // Every visible panel rebuild
        foreach (var config in panels)
        {
            if (config.panelRoot != null && config.panelRoot.activeSelf)
            {
                var panelUI = config.panelRoot.GetComponent<InventoryPanelUI>();
                if (panelUI != null)
                {
                    panelUI.Refresh(currentTargetId, currentPlayerId);
                }
            }
        }
    }
}