using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StorageComponent))]
public class StorageInteractable : InteractableComponent, IInjectable
{
    [Header("UI Configuration")]
    [SerializeField] private List<StoragePanelType> panelsToOpen = new List<StoragePanelType>
    {
        StoragePanelType.BigInOut
    };

    [Header("Interaction Settings")]
    [SerializeField] private float closeDistance = 5f;

    private StorageComponent myStorage;

    private bool isInteracting = false;
    private GameObject currentInteractor;

    private InventoryUIManager uiManager;

    public void Inject(DependencyContainer container)
    {
        uiManager = container.Get<InventoryUIManager>();
    }

    protected override void Awake()
    {
        base.Awake();
        myStorage = GetComponent<StorageComponent>();
    }

    protected override void ExecuteInteraction(GameObject interactor)
    {
        StorageComponent playerStorage = interactor.GetComponent<StorageComponent>();

        if (playerStorage != null && uiManager != null)
        {
            // Open panel 
            uiManager.OpenUI(
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