using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StorageComponent))]
public class DropBuildingInteractable : InteractableComponent
{
    [Serializable]
    public struct TierSlot
    {
        public int resourceId;
        public int amount;
    }

    [Serializable]
    public struct DropTier
    {
        public string tierName;
        public List<TierSlot> inputs;  // Requirements
        public List<TierSlot> outputs; // Rewards
    }

    [Header("Tier Progression")]
    public List<DropTier> buildingTiers = new List<DropTier>();
    private int currentTierIndex = 0;

    [Header("UI Configuration")]
    public List<StoragePanelType> panelsToOpen = new List<StoragePanelType>
    {
        StoragePanelType.SmallInOut,
        StoragePanelType.Out,
        StoragePanelType.Launch
    };

    [Header("Interaction Settings")]
    public float closeDistance = 5f;

    private StorageComponent myStorage;
    private bool isInteracting = false;
    private GameObject currentInteractor;


    // Track slots so clean up when switching tiers
    private List<int> activeResourceIds = new List<int>();

    // Tracks if waiting for player to collect reward
    private bool isWaitingForCollection = false;

    protected override void Awake()
    {
        base.Awake();
        myStorage = GetComponent<StorageComponent>();
    }

    private void Start()
    {
        LoadTier(currentTierIndex);

        LaunchPanelUI.OnLaunchPressed += HandleLaunch;
    }

    private void OnDestroy()
    {
        LaunchPanelUI.OnLaunchPressed -= HandleLaunch;
    }

    private void LoadTier(int index)
    {
        if (index >= buildingTiers.Count) return;

        DropTier tier = buildingTiers[index];

        // Clean up
        foreach (int oldId in activeResourceIds)
        {
            GameManager.ResourceHub.SetupResourceSlot(myStorage.StorageID, oldId, 0, false, false, true);
        }
        activeResourceIds.Clear();

        // Set up requirements 
        foreach (var input in tier.inputs)
        {
            GameManager.ResourceHub.SetupResourceSlot(myStorage.StorageID, input.resourceId, input.amount, true, true, true);
            activeResourceIds.Add(input.resourceId);
        }

        // Set up rewards
        foreach (var output in tier.outputs)
        {
            GameManager.ResourceHub.SetupResourceSlot(myStorage.StorageID, output.resourceId, output.amount, false, true, true);
            activeResourceIds.Add(output.resourceId);

            // Force output slots to start empty
            GameManager.ResourceHub.ConsumeResource(myStorage.StorageID, output.resourceId, 9999);
        }
    }

    private void HandleLaunch()
    {
        if (!isInteracting) return;
        if (currentTierIndex >= buildingTiers.Count) return;
        if (isWaitingForCollection) return;

        if (AreInputsFull())
        {
            DropTier currentTier = buildingTiers[currentTierIndex];

            // Use required items
            foreach (var input in currentTier.inputs)
            {
                GameManager.ResourceHub.ConsumeResource(myStorage.StorageID, input.resourceId, input.amount, true);

                // Lock so can't use the required item slot
                GameManager.ResourceHub.SetupResourceSlot(myStorage.StorageID, input.resourceId, input.amount, false, false, true);
            }

            // Give reward
            foreach (var output in currentTier.outputs)
            {
                GameManager.ResourceHub.AddResource(myStorage.StorageID, output.resourceId, output.amount, true);
            }

            isWaitingForCollection = true;
        }
    }

    private bool AreInputsFull()
    {
        if (currentTierIndex >= buildingTiers.Count) return false;

        DropTier currentTier = buildingTiers[currentTierIndex];
        var inventory = GameManager.ResourceHub.GetReadOnlyInventory(myStorage.StorageID);

        if (inventory == null) return false;

        foreach (var input in currentTier.inputs)
        {
            if (inventory.TryGetValue(input.resourceId, out ResourceState state))
            {
                if (state.current < state.max) return false; // Missing items
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    private bool AreOutputsEmpty()
    {
        if (currentTierIndex >= buildingTiers.Count) return true;

        DropTier currentTier = buildingTiers[currentTierIndex];
        var inventory = GameManager.ResourceHub.GetReadOnlyInventory(myStorage.StorageID);

        if (inventory == null) return true;

        foreach (var output in currentTier.outputs)
        {
            if (inventory.TryGetValue(output.resourceId, out ResourceState state))
            {
                if (state.current > 0) return false; // Not empty yet
            }
        }
        return true;
    }

    protected override void ExecuteInteraction(GameObject interactor)
    {
        StorageComponent playerStorage = interactor.GetComponent<StorageComponent>();

        if (playerStorage != null)
        {
            InventoryUIManager.Instance.OpenUI(myStorage.StorageID, playerStorage.StorageID, panelsToOpen);
            currentInteractor = interactor;
            isInteracting = true;
        }
    }

    private void Update()
    {
        if (isInteracting && currentInteractor != null)
        {
            // Close if player walks away
            if (Vector3.Distance(transform.position, currentInteractor.transform.position) > closeDistance)
            {
                StopInteraction();
            }

            if (isWaitingForCollection)
            {
                // Launch button disabled while waiting for player to collect reward
                if (LaunchPanelUI.Instance != null && LaunchPanelUI.Instance.gameObject.activeInHierarchy)
                {
                    LaunchPanelUI.Instance.SetLaunchInteractable(false);
                }

                // Check if grabbed everything
                if (AreOutputsEmpty())
                {
                    // Next tier
                    isWaitingForCollection = false;
                    currentTierIndex++;
                    LoadTier(currentTierIndex);
                }
            }
            else
            {
                // Enable launch button if met requirements
                if (LaunchPanelUI.Instance != null && LaunchPanelUI.Instance.gameObject.activeInHierarchy)
                {
                    LaunchPanelUI.Instance.SetLaunchInteractable(AreInputsFull());
                }
            }
        }
    }

    private void StopInteraction()
    {
        isInteracting = false;
        currentInteractor = null;

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.CloseUI();
        }
    }
}