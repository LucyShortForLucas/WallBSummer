using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUIRow : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text amountText;

    [Header("Buttons")]
    public Button storeOneBtn;
    public Button storeAllBtn;
    public Button takeOneBtn;
    public Button takeAllBtn;

    private int targetStorageId;
    private int playerStorageId;
    private int resourceId;

    public void Setup(GlobalResourceDatabase.ResourceDefinition resDef, ResourceState state, int targetId, int playerId, StoragePanelType panelType)
    {
        resourceId = resDef.id;
        targetStorageId = targetId;
        playerStorageId = playerId;

        // Setup the visuals
        iconImage.sprite = resDef.icon;
        nameText.text = resDef.resourceName;
        amountText.text = $"{state.current} / {state.max}";

        // Connect all our transfer buttons
        if (storeOneBtn != null) storeOneBtn.onClick.AddListener(() => StoreAmount(1));
        if (takeOneBtn != null) takeOneBtn.onClick.AddListener(() => TakeAmount(1));

        if (storeAllBtn != null) storeAllBtn.onClick.AddListener(() => StoreAmount(GetAvailablePlayerAmount(resourceId)));
        if (takeAllBtn != null) takeAllBtn.onClick.AddListener(() => TakeAmount(state.current));
    }

    private void StoreAmount(int amount)
    {
        if (amount > 0) GameManager.ResourceHub.TransferResource(playerStorageId, targetStorageId, resourceId, amount);
    }

    private void TakeAmount(int amount)
    {
        if (amount > 0) GameManager.ResourceHub.TransferResource(targetStorageId, playerStorageId, resourceId, amount);
    }

    private int GetAvailablePlayerAmount(int resId)
    {
        // Check how much of this item the player has
        var playerInv = GameManager.ResourceHub.GetReadOnlyInventory(playerStorageId);

        if (playerInv != null && playerInv.TryGetValue(resId, out ResourceState state))
        {
            return state.current;
        }
        return 0;
    }
}