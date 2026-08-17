using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIRow : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text amountText;

    [Header("Buttons")]
    [SerializeField] private Button storeOneBtn;
    [SerializeField] private Button storeAllBtn;
    [SerializeField] private Button takeOneBtn;
    [SerializeField] private Button takeAllBtn;

    private int targetStorageId;
    private int playerStorageId;
    private int resourceId;

    private CentralResourceHub resourceHub;

    public void Setup(GlobalResourceDatabase.ResourceDefinition resDef, ResourceState state, int targetId, int playerId, StoragePanelType panelType, CentralResourceHub hub)
    {
        resourceId = resDef.id;
        targetStorageId = targetId;
        playerStorageId = playerId; 
        resourceHub = hub; 

        iconImage.sprite = resDef.icon;
        nameText.text = resDef.resourceName;
        amountText.text = $"{state.current} / {state.max}";

        if (storeOneBtn != null) storeOneBtn.onClick.AddListener(() => StoreAmount(1));
        if (takeOneBtn != null) takeOneBtn.onClick.AddListener(() => TakeAmount(1));

        if (storeAllBtn != null) storeAllBtn.onClick.AddListener(() => StoreAmount(GetAvailablePlayerAmount(resourceId)));
        if (takeAllBtn != null) takeAllBtn.onClick.AddListener(() => TakeAmount(state.current));
    }

    private void StoreAmount(int amount)
    {
        if (amount > 0) resourceHub.TransferResource(playerStorageId, targetStorageId, resourceId, amount); 
    }

    private void TakeAmount(int amount)
    {
        if (amount > 0) resourceHub.TransferResource(targetStorageId, playerStorageId, resourceId, amount); 
    }

    private int GetAvailablePlayerAmount(int resId)
    {
        var playerInv = resourceHub.GetReadOnlyInventory(playerStorageId);

        if (playerInv != null && playerInv.TryGetValue(resId, out ResourceState state))
        {
            return state.current;
        }
        return 0;
    }
}