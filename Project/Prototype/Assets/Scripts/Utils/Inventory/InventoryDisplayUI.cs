using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryDisplayUI : MonoBehaviour, IInjectable
{
    [Header("Configuration")]
    [SerializeField] private StorageComponent playerStorage;
    [SerializeField] private int resourceId;

    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;

    private CentralResourceHub resourceHub;
    private InventoryUIManager uiManager; 

    public void Inject(DependencyContainer container)
    {
        resourceHub = container.Get<CentralResourceHub>();
        uiManager = container.Get<InventoryUIManager>();

        // Setup icon
        if (uiManager != null && uiManager.ResourceDatabase != null)
        {
            var resDef = uiManager.ResourceDatabase.GetResource(resourceId);
            if (iconImage != null && resDef.icon != null)
            {
                iconImage.sprite = resDef.icon;
            }
        }

        if (resourceHub != null)
        {
            resourceHub.OnResourceChanged += HandleResourceChanged;
        }
    }

    private void Start()
    {
        RefreshAmount();
    }

    private void OnDestroy()
    {
        if (resourceHub != null)
        {
            resourceHub.OnResourceChanged -= HandleResourceChanged;
        }
    }

    private void HandleResourceChanged(int storageId, int changedResId, int newAmount, int maxAmount)
    {
        // Update UI if change happened to player and resource
        if (playerStorage != null && storageId == playerStorage.StorageID && changedResId == resourceId)
        {
            if (amountText != null)
            {
                amountText.text = $"{newAmount} / {maxAmount}";
            }
        }
    }

    private void RefreshAmount()
    {
        if (resourceHub == null || playerStorage == null) return;

        var inventory = resourceHub.GetReadOnlyInventory(playerStorage.StorageID);

        if (inventory != null && inventory.TryGetValue(resourceId, out ResourceState state))
        {
            if (amountText != null) amountText.text = $"{state.current} / {state.max}";
        }
        else
        {
            if (amountText != null) amountText.text = "0 / 0";
        }
    }
}
