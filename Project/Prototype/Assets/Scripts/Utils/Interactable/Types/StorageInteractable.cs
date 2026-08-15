using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StorageComponent))]
public class StorageInteractable : InteractableComponent
{
    [Header("UI Configuration")]
    private List<StoragePanelType> panelsToOpen = new List<StoragePanelType>
    {
        StoragePanelType.BigInOut
    };

    [Header("Interaction Settings")]
    public float closeDistance = 5f;

    private StorageComponent myStorage;

    private bool isInteracting = false;
    private GameObject currentInteractor;

    protected override void Awake()
    {
        base.Awake();
        myStorage = GetComponent<StorageComponent>();
    }

    protected override void ExecuteInteraction(GameObject interactor)
    {
        StorageComponent playerStorage = interactor.GetComponent<StorageComponent>();

        if (playerStorage != null)
        {
            // Open panel
            InventoryUIManager.Instance.OpenUI(
                myStorage.StorageID,
                playerStorage.StorageID,
                panelsToOpen
            );

            currentInteractor = interactor;
            isInteracting = true;
        }
    }

    private void Update()
    {
        if (isInteracting && currentInteractor != null)
        {
            float distance = Vector3.Distance(transform.position, currentInteractor.transform.position);

            // Close if player walks away
            if (distance > closeDistance)
            {
                StopInteraction();
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