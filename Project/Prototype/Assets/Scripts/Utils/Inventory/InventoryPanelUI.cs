using System.Collections.Generic;
using UnityEngine;

public class InventoryPanelUI : MonoBehaviour
{
    [Header("Panel Settings")]
    public StoragePanelType panelType;
    public Transform rowContainer;
    public GameObject rowPrefab;

    private List<GameObject> activeRows = new List<GameObject>();

    public void Refresh(int targetStorageId, int playerStorageId)
    {
        // Clean
        foreach (var row in activeRows) Destroy(row);
        activeRows.Clear();

        if (rowContainer == null || rowPrefab == null) return;

        // Grab data
        var inventory = GameManager.ResourceHub.GetReadOnlyInventory(targetStorageId);
        if (inventory == null) return;

        // Build UI
        foreach (var kvp in inventory)
        {
            int resId = kvp.Key;
            ResourceState state = kvp.Value;

            bool shouldSpawnRow = false;

            switch (panelType)
            {
                case StoragePanelType.Out:
                    // Display items, allowed to take and not store
                    shouldSpawnRow = (state.allowTaking && !state.allowStoring);
                    break;

                case StoragePanelType.BigInOut:
                case StoragePanelType.SmallInOut:
                    // Display standard storage items that allow full swapping
                    shouldSpawnRow = (state.allowTaking && state.allowStoring);
                    break;
            }

            // Skip if not allowed
            if (!shouldSpawnRow)
            {
                continue;
            }

            var resDef = InventoryUIManager.Instance.resourceDatabase.GetResource(resId);
            if (string.IsNullOrEmpty(resDef.resourceName)) continue;

            // Create row
            GameObject newRow = Instantiate(rowPrefab, rowContainer);
            newRow.SetActive(true);
            activeRows.Add(newRow);

            var rowScript = newRow.GetComponent<InventoryUIRow>();
            rowScript.Setup(resDef, state, targetStorageId, playerStorageId, panelType);
        }
    }

    public void Clear()
    {
        foreach (var row in activeRows) Destroy(row);
        activeRows.Clear();
    }
}