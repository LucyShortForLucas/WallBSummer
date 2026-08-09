using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceUIRow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;

    [Header("Buttons")]
    [SerializeField] private Button storeAllButton;
    [SerializeField] private Button addOneButton;
    [SerializeField] private Button takeOneButton;
    [SerializeField] private Button takeAllButton;

    private ResourceData resource;
    private ResourceStorage playerStorage;
    private ResourceStorage buildingStorage;

    public void Setup(ResourceData res, ResourceStorage player, ResourceStorage building)
    {
        resource = res;
        playerStorage = player;
        buildingStorage = building;

        iconImage.sprite = res.icon;
        nameText.text = res.resourceName;

        // Clear listeners and add new ones 
        storeAllButton.onClick.RemoveAllListeners();
        storeAllButton.onClick.AddListener(() => Transfer(playerStorage, buildingStorage, int.MaxValue));

        addOneButton.onClick.RemoveAllListeners();
        addOneButton.onClick.AddListener(() => Transfer(playerStorage, buildingStorage, 1));

        takeOneButton.onClick.RemoveAllListeners();
        takeOneButton.onClick.AddListener(() => Transfer(buildingStorage, playerStorage, 1));

        takeAllButton.onClick.RemoveAllListeners();
        takeAllButton.onClick.AddListener(() => Transfer(buildingStorage, playerStorage, int.MaxValue));

        // Buttons active state if is allowed
        storeAllButton.interactable = buildingStorage.CanStore(resource);
        addOneButton.interactable = buildingStorage.CanStore(resource);

        takeOneButton.interactable = buildingStorage.CanTake(resource);
        takeAllButton.interactable = buildingStorage.CanTake(resource);

        // Subscribe to resource change events to update the UI when amounts change
        buildingStorage.OnResourceChanged += HandleResourceChanged;
        UpdateText();
    }

    private void OnDestroy()
    {
        if (buildingStorage != null)
        {
            buildingStorage.OnResourceChanged -= HandleResourceChanged;
        }
    }

    private void HandleResourceChanged(ResourceData res, int current, int max)
    {
        if (res == resource)
        {
            UpdateText();
        }
    }

    private void UpdateText()
    {
        amountText.text = $"{buildingStorage.GetAmount(resource)}/{buildingStorage.GetMaxAmount(resource)}";
    }

    // Transfer resources from source to destination, checking the available amounts and max capacity
    private void Transfer(ResourceStorage source, ResourceStorage destination, int requestedAmount)
    {
        int availableSource = source.GetAmount(resource);
        int availableDestSpace = destination.GetMaxAmount(resource) - destination.GetAmount(resource);

        int actualTransfer = Mathf.Min(requestedAmount, availableSource, availableDestSpace);

        if (actualTransfer > 0 && source.ConsumeResource(resource, actualTransfer))
        {
            destination.AddResource(resource, actualTransfer);
        }
    }
}