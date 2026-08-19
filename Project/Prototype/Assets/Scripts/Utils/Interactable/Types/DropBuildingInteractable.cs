using System;
using System.Collections.Generic;
using UnityEngine;

public class DropBuildingInteractable : InteractableComponent, IInjectable
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
        public List<TierSlot> inputs;
        public List<TierSlot> outputs;
    }

    [Header("Tier Progression")]
    [SerializeField] private List<DropTier> buildingTiers = new List<DropTier>();
    private int currentTierIndex = 0;

    [Header("Storage Configuration")]
    [SerializeField] private StorageComponent inventoryStorage;
    [SerializeField] private StorageComponent outputStorage;

    [Header("Interaction Settings")]
    [SerializeField] private float closeDistance = 5f;

    private bool isInteracting = false;
    private GameObject currentInteractor;

    private List<int> activeResourceIds = new List<int>();
    private bool isWaitingForCollection = false;

    private CentralResourceHub resourceHub;
    private InventoryUIManager uiManager;

    public bool CanLaunch => AreInputsFull();
    public bool IsWaiting => isWaitingForCollection;

    public void Inject(DependencyContainer container)
    {
        resourceHub = container.Get<CentralResourceHub>();
        uiManager = container.Get<InventoryUIManager>();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        LoadTier(currentTierIndex);
    }

    private void LoadTier(int index)
    {
        if (index >= buildingTiers.Count || resourceHub == null) return;

        DropTier tier = buildingTiers[index];

        // Clear
        foreach (int oldId in activeResourceIds)
        {
            resourceHub.SetupResourceSlot(inventoryStorage.StorageID, oldId, 0, false, false, true);
            resourceHub.SetupResourceSlot(outputStorage.StorageID, oldId, 0, false, false, true);
        }
        activeResourceIds.Clear();

        // Set new inputs
        foreach (var input in tier.inputs)
        {
            resourceHub.SetupResourceSlot(inventoryStorage.StorageID, input.resourceId, input.amount, true, true, true);
            activeResourceIds.Add(input.resourceId);
        }

        // Set new outputs
        foreach (var output in tier.outputs)
        {
            resourceHub.SetupResourceSlot(outputStorage.StorageID, output.resourceId, output.amount, false, true, true);
            activeResourceIds.Add(output.resourceId);
            resourceHub.ConsumeResource(outputStorage.StorageID, output.resourceId, 9999);
        }
    }

    public void TriggerLaunch()
    {
        if (!isInteracting || resourceHub == null) return;
        if (currentTierIndex >= buildingTiers.Count) return;
        if (isWaitingForCollection) return;

        if (AreInputsFull())
        {
            DropTier currentTier = buildingTiers[currentTierIndex];

            // Consume inputs
            foreach (var input in currentTier.inputs)
            {
                resourceHub.ConsumeResource(inventoryStorage.StorageID, input.resourceId, input.amount, true);
                resourceHub.SetupResourceSlot(inventoryStorage.StorageID, input.resourceId, input.amount, false, false, true);
            }

            // Generate outputs
            foreach (var output in currentTier.outputs)
            {
                resourceHub.AddResource(outputStorage.StorageID, output.resourceId, output.amount, true);
            }

            isWaitingForCollection = true;
        }
    }

    private bool AreInputsFull()
    {
        if (currentTierIndex >= buildingTiers.Count || resourceHub == null) return false;

        DropTier currentTier = buildingTiers[currentTierIndex];
        var inventory = resourceHub.GetReadOnlyInventory(inventoryStorage.StorageID);

        if (inventory == null) return false;

        // Check if all input resources are at max
        foreach (var input in currentTier.inputs)
        {
            if (inventory.TryGetValue(input.resourceId, out ResourceState state))
            {
                if (state.current < state.max) return false;
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
        if (currentTierIndex >= buildingTiers.Count || resourceHub == null) return true;

        DropTier currentTier = buildingTiers[currentTierIndex];
        var inventory = resourceHub.GetReadOnlyInventory(outputStorage.StorageID);

        if (inventory == null) return true;

        // Check if all output resources are empty
        foreach (var output in currentTier.outputs)
        {
            if (inventory.TryGetValue(output.resourceId, out ResourceState state))
            {
                if (state.current > 0) return false;
            }
        }
        return true;
    }

    protected override void ExecuteInteraction(GameObject interactor)
    {
        StorageComponent playerStorage = interactor.GetComponent<StorageComponent>();

        // Open UI
        if (playerStorage != null && uiManager != null)
        {
            uiManager.CurrentDropBuilding = this;

            Dictionary<StoragePanelType, int> panelRequests = new Dictionary<StoragePanelType, int>
            {
                { StoragePanelType.SmallInOut, inventoryStorage.StorageID },
                { StoragePanelType.Out, outputStorage.StorageID },
                { StoragePanelType.Launch, 0 }
            };

            uiManager.OpenUI(playerStorage.StorageID, panelRequests);
            currentInteractor = interactor;
            isInteracting = true;
        }
    }

    private void Update()
    {
        if (isInteracting && currentInteractor != null && uiManager != null)
        {
            // Close if player walks away
            if (Vector3.Distance(transform.position, currentInteractor.transform.position) > closeDistance)
            {
                StopInteraction();
            }

            // Check if empty then next tier
            if (isWaitingForCollection)
            {
                if (AreOutputsEmpty())
                {
                    isWaitingForCollection = false;
                    currentTierIndex++;
                    LoadTier(currentTierIndex);
                }
            }
        }
    }

    private void StopInteraction()
    {
        // Reset interaction states and hide UI
        isInteracting = false;
        currentInteractor = null;

        if (uiManager != null)
        {
            uiManager.CloseUI();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, closeDistance);
    }
#endif
}