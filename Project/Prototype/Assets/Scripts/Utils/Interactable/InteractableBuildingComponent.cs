using UnityEngine;

public class InteractableBuildingComponent : InteractableComponent
{
    private InteractableUIController uiController;
    private ResourceStorage buildingStorage;

    private GameObject currentInteractor;

    protected override void Awake()
    {
        base.Awake();
        buildingStorage = GetComponent<ResourceStorage>();
        uiController = Object.FindAnyObjectByType<InteractableUIController>(FindObjectsInactive.Include);
    }

    protected override void ExecuteInteraction(GameObject interactor)
    {
        ResourceStorage playerStorage = interactor.GetComponent<ResourceStorage>();

        // Open interaction panel
        if (playerStorage != null && buildingStorage != null && uiController != null)
        {
            currentInteractor = interactor;
            uiController.OpenPanel(playerStorage, buildingStorage);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Close panel if interactor leaves trigger area
        if (currentInteractor != null && other.gameObject == currentInteractor)
        {
            if (uiController != null)
            {
                uiController.ClosePanel();
            }
            currentInteractor = null;
        }
    }
}