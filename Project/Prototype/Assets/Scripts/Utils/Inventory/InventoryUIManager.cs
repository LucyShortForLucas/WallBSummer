using System;
using System.Collections.Generic;
using UnityEngine;

public enum StoragePanelType
{
    BigInOut,
    SmallInOut,
    Out,
    Launch,
    Recipes, 
    Craft,
    WaterUsage
}

public class InventoryUIManager : MonoBehaviour, IInjectable
{
    [Header("Data")]
    [SerializeField] private GlobalResourceDatabase resourceDatabase;
    [SerializeField] private GlobalRecipeDatabase recipeDatabase;


    [Serializable]
    public struct PanelConfig
    {
        public StoragePanelType panelType;
        public GameObject panelRoot;
    }

    [Header("Panel Mappings")]
    [SerializeField] private List<PanelConfig> panels = new List<PanelConfig>();

    private int currentPlayerId = -1;

    private Dictionary<StoragePanelType, int> activePanels = new Dictionary<StoragePanelType, int>();

    private CentralResourceHub resourceHub;

    // Getters and Setters
    public GlobalResourceDatabase ResourceDatabase { get => resourceDatabase; set => resourceDatabase = value; }
    public FactoryInteractable CurrentFactory { get; set; }
    public DropBuildingInteractable CurrentDropBuilding { get; set; }

    public void Inject(DependencyContainer container)
    {
        resourceHub = container.Get<CentralResourceHub>();
        resourceHub.OnResourceChanged += HandleResourceChanged;
    }

    private void Awake()
    {
        if (resourceDatabase != null) resourceDatabase.Initialize(); // BAD, databases should not require runtime initialization, this is why they're scriptable objects in the first place. 
        if (recipeDatabase != null) recipeDatabase.Initialize();    // This currently make them (and things that rely on them) untestable -Lucy

        CloseAllPanels();
    }

    private void OnDestroy()
    {
        if (resourceHub != null)
        {
            resourceHub.OnResourceChanged -= HandleResourceChanged;
        }
    }

    public void OpenUI(int playerStorageId, Dictionary<StoragePanelType, int> requestedPanels)
    {
        currentPlayerId = playerStorageId;
        activePanels = new Dictionary<StoragePanelType, int>(requestedPanels);

        CloseAllPanels();

        // Turn on requested panels
        foreach (var kvp in activePanels)
        {
            foreach (var config in panels)
            {
                if (config.panelType == kvp.Key && config.panelRoot != null)
                {
                    config.panelRoot.SetActive(true);
                    break;
                }
            }
        }

        RefreshUI();
    }

    public void CloseUI()
    {
        // Hide everything and reset our tracking variables
        CloseAllPanels();
        activePanels.Clear();
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
        if (storageId == currentPlayerId || activePanels.ContainsValue(storageId))
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        foreach (var config in panels)
        {
            if (config.panelRoot != null && config.panelRoot.activeSelf)
            {
                // Inventory Panels
                var panelUI = config.panelRoot.GetComponent<InventoryPanelUI>();
                if (panelUI != null && activePanels.TryGetValue(config.panelType, out int targetId))
                {
                    panelUI.Refresh(targetId, currentPlayerId, resourceHub, resourceDatabase);
                }

                // Recipe Panels
                var recipePanel = config.panelRoot.GetComponent<RecipePanelUI>();
                if (recipePanel != null) recipePanel.Refresh(this, recipeDatabase);

                // Crafting Panels
                var craftPanel = config.panelRoot.GetComponent<CraftPanelUI>();
                if (craftPanel != null) craftPanel.Refresh(this, resourceHub);

                // Launch Panels 
                var launchPanel = config.panelRoot.GetComponent<LaunchPanelUI>();
                if (launchPanel != null) launchPanel.Refresh(this);

                // Water Usage Panels
                var waterPanel = config.panelRoot.GetComponent<WaterUsagePanelUI>();
                if (waterPanel != null) waterPanel.Refresh(this);
            }
        }
    }
}