using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StorageComponent))]
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

    [Header("UI Configuration")]
    [SerializeField] private List<StoragePanelType> panelsToOpen = new List<StoragePanelType>
    {
        StoragePanelType.SmallInOut,
        StoragePanelType.Out,
        StoragePanelType.Launch
    };

    [Header("Interaction Settings")]
    [SerializeField] private float closeDistance = 5f;

    private StorageComponent myStorage;
    private bool isInteracting = false;
    private GameObject currentInteractor;

    private List<int> activeResourceIds = new List<int>();
    private bool isWaitingForCollection = false;

    private CentralResourceHub resourceHub;
    private InventoryUIManager uiManager;
    private LaunchPanelUI launchUI;

    public void Inject(DependencyContainer container)
    {
        resourceHub = container.Get<CentralResourceHub>();
        uiManager = container.Get<InventoryUIManager>();
        launchUI = container.Get<LaunchPanelUI>();

        if (launchUI != null)
        {
            launchUI.OnLaunchPressed += HandleLaunch;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        myStorage = GetComponent<StorageComponent>();
    }

    private void Start()
    {
        LoadTier(currentTierIndex);
    }

    private void OnDestroy()
    {
        if (launchUI != null)
        {
            launchUI.OnLaunchPressed -= HandleLaunch;
        }
    }

    private void LoadTier(int index)
    {
        if (index >= buildingTiers.Count || resourceHub == null) return;

        DropTier tier = buildingTiers[index];

        // Clear
        foreach (int oldId in activeResourceIds)
        {
            resourceHub.SetupResourceSlot(myStorage.StorageID, oldId, 0, false, false, true);
        }
        activeResourceIds.Clear();

        // Set new inputs
        foreach (var input in tier.inputs)
        {
            resourceHub.SetupResourceSlot(myStorage.StorageID, input.resourceId, input.amount, true, true, true);
            activeResourceIds.Add(input.resourceId);
        }

        // Set new outputs
        foreach (var output in tier.outputs)
        {
            resourceHub.SetupResourceSlot(myStorage.StorageID, output.resourceId, output.amount, false, true, true);
            activeResourceIds.Add(output.resourceId);
            resourceHub.ConsumeResource(myStorage.StorageID, output.resourceId, 9999);
        }
    }

    private void HandleLaunch()
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
                resourceHub.ConsumeResource(myStorage.StorageID, input.resourceId, input.amount, true);
                resourceHub.SetupResourceSlot(myStorage.StorageID, input.resourceId, input.amount, false, false, true);
            }

            // Generate outputs
            foreach (var output in currentTier.outputs)
            {
                resourceHub.AddResource(myStorage.StorageID, output.resourceId, output.amount, true);
            }

            isWaitingForCollection = true;
        }
    }

    private bool AreInputsFull()
    {
        if (currentTierIndex >= buildingTiers.Count || resourceHub == null) return false;

        DropTier currentTier = buildingTiers[currentTierIndex];
        var inventory = resourceHub.GetReadOnlyInventory(myStorage.StorageID);

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
        var inventory = resourceHub.GetReadOnlyInventory(myStorage.StorageID);

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
            uiManager.OpenUI(myStorage.StorageID, playerStorage.StorageID, panelsToOpen);
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

            // Update launch button state based on current tier status
            if (isWaitingForCollection)
            {
                if (launchUI != null) launchUI.SetLaunchInteractable(false);

                if (AreOutputsEmpty())
                {
                    isWaitingForCollection = false;
                    currentTierIndex++;
                    LoadTier(currentTierIndex);
                }
            }
            else
            {
                if (launchUI != null) launchUI.SetLaunchInteractable(AreInputsFull());
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