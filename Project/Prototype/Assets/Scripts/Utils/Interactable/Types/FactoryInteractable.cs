using System.Collections.Generic;
using UnityEngine;
using System;

public class FactoryInteractable : InteractableComponent, IInjectable
{
    [Header("Storage Configuration")]
    [SerializeField] private StorageComponent inputStorage;
    [SerializeField] private StorageComponent outputStorage;

    [Header("Factory Configuration")]
    [SerializeField] private List<int> allowedRecipeIds = new List<int>();

    [Header("Interaction Settings")]
    [SerializeField] private float closeDistance = 5f;

    public event Action OnOpenFactory;
    public event Action OnCloseFactory;

    private bool isInteracting = false;
    private GameObject currentInteractor;

    private InventoryUIManager uiManager;
    private CentralResourceHub resourceHub;

    // Getters and Setters
    public StorageComponent InputStorage { get => inputStorage; }
    public StorageComponent OutputStorage { get => outputStorage; }
    public List<int> AllowedRecipeIds { get => allowedRecipeIds; }

    public void Inject(DependencyContainer container)
    {
        uiManager = container.Get<InventoryUIManager>();
        resourceHub = container.Get<CentralResourceHub>(); 
    }

    protected override void ExecuteInteraction(GameObject interactor)
    {
        StorageComponent playerStorage = interactor.GetComponent<StorageComponent>();

        // Open UI
        if (playerStorage != null && uiManager != null)
        {
            uiManager.CurrentFactory = this;

            Dictionary<StoragePanelType, int> panelRequests = new Dictionary<StoragePanelType, int>
            {
                { StoragePanelType.SmallInOut, inputStorage.StorageID },
                { StoragePanelType.Out, outputStorage.StorageID },
                { StoragePanelType.Recipes, 0 }, 
                { StoragePanelType.Craft, 0 }    
            };

            uiManager.OpenUI(playerStorage.StorageID, panelRequests);
            currentInteractor = interactor;
            isInteracting = true;
        }
        OnOpenFactory?.Invoke();
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
        }
    }

    private void StopInteraction()
    {
        isInteracting = false;
        currentInteractor = null;

        // Close UI
        if (uiManager != null)
        {
            uiManager.CloseUI();
            OnCloseFactory?.Invoke();
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