using System.Collections.Generic;
using UnityEngine;

public class InventoryPanelUI : MonoBehaviour
{
    [Header("Panel Settings")]
    [SerializeField] private StoragePanelType panelType;
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;

    private List<GameObject> activeRows = new List<GameObject>();

    public void Refresh(int targetStorageId, int playerStorageId, CentralResourceHub hub, GlobalResourceDatabase database)
    {
        // Clear
        foreach (var row in activeRows) Destroy(row);
        activeRows.Clear();

        if (rowContainer == null || rowPrefab == null) return;

        // Get Data
        var inventory = hub.GetReadOnlyInventory(targetStorageId);
        if (inventory == null) return;

        // Select panel and create info
        foreach (var kvp in inventory)
        {
            int resId = kvp.Key;
            ResourceState state = kvp.Value;

            bool shouldSpawnRow = false;

            switch (panelType)
            {
                case StoragePanelType.Out:
                    shouldSpawnRow = (state.allowTaking && !state.allowStoring);
                    break;
                case StoragePanelType.BigInOut:
                case StoragePanelType.SmallInOut:
                    shouldSpawnRow = (state.allowTaking && state.allowStoring);
                    break;
            }

            if (!shouldSpawnRow) continue;

            var resDef = database.GetResource(resId);
            if (string.IsNullOrEmpty(resDef.resourceName)) continue;

            GameObject newRow = Instantiate(rowPrefab, rowContainer);
            newRow.SetActive(true);
            activeRows.Add(newRow);

            var rowScript = newRow.GetComponent<InventoryUIRow>();

            rowScript.Setup(resDef, state, targetStorageId, playerStorageId, panelType, hub);
        }
    }

    public void Clear()
    {
        foreach (var row in activeRows) Destroy(row);
        activeRows.Clear();
    }
}