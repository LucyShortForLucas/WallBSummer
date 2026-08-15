using System;
using System.Collections.Generic;
using UnityEngine;

public class CentralResourceHub
{
    // Notify changes
    public event Action<int, int, int, int> OnResourceChanged;

    // Stores inventory data for each active storage
    private Dictionary<int, InventoryData> activeStorages = new Dictionary<int, InventoryData>();

    // New unique ID for new storage
    private int nextStorageId = 1;


    public int RegisterStorage(bool isPlayerOwned)
    {
        int newId = nextStorageId++;
        activeStorages[newId] = new InventoryData(newId, isPlayerOwned);
        return newId;
    }

    public void UnregisterStorage(int storageId)
    {
        if (activeStorages.ContainsKey(storageId))
        {
            activeStorages.Remove(storageId);
        }
    }

    // Creates or updates resource slot in storage
    public void SetupResourceSlot(int storageId, int resourceId, int maxCapacity = 100, bool canStore = true, bool canTake = true, bool overwrite = false)
    {
        if (activeStorages.TryGetValue(storageId, out InventoryData inv))
        {
            if (!inv.resources.ContainsKey(resourceId))
            {
                inv.resources[resourceId] = new ResourceState(maxCapacity, canStore, canTake);
            }
            else if (overwrite)
            {
                // Refresh the slot settings for an upgrade
                ResourceState state = inv.resources[resourceId];
                state.max = maxCapacity;
                state.allowStoring = canStore;
                state.allowTaking = canTake;

                // Keep the current amount within the new limit
                state.current = Mathf.Min(state.current, state.max);
            }

            // Notify about resource state
            OnResourceChanged?.Invoke(storageId, resourceId, inv.resources[resourceId].current, maxCapacity);
        }
    }


    public void AddResource(int storageId, int resourceId, int amount, bool bypassPermissions = false)
    {
        if (!activeStorages.TryGetValue(storageId, out InventoryData inv)) return;
        if (!inv.resources.TryGetValue(resourceId, out ResourceState state)) return;
        if (!state.allowStoring && !bypassPermissions) return;

        state.current += amount;
        state.current = Mathf.Min(state.current, state.max);

        OnResourceChanged?.Invoke(storageId, resourceId, state.current, state.max);
    }

    public bool ConsumeResource(int storageId, int resourceId, int amount, bool bypassPermissions = false)
    {
        if (!activeStorages.TryGetValue(storageId, out InventoryData inv)) return false;
        if (!inv.resources.TryGetValue(resourceId, out ResourceState state)) return false;
        if (!state.allowTaking && !bypassPermissions) return false;

        if (state.current >= amount)
        {
            state.current -= amount;
            OnResourceChanged?.Invoke(storageId, resourceId, state.current, state.max);
            return true;
        }

        return false;
    }

    public bool HasEnough(int storageId, int resourceId, int amount)
    {
        if (activeStorages.TryGetValue(storageId, out InventoryData inv) &&
            inv.resources.TryGetValue(resourceId, out ResourceState state))
        {
            return state.current >= amount;
        }
        return false;
    }

    public bool TransferResource(int sourceId, int targetId, int resourceId, int amount)
    {
        // Check if both storages exist
        if (!activeStorages.TryGetValue(sourceId, out InventoryData sourceInv)) return false;
        if (!activeStorages.TryGetValue(targetId, out InventoryData targetInv)) return false;

        // Check if both storages have resource
        if (!sourceInv.resources.TryGetValue(resourceId, out ResourceState sourceState)) return false;
        if (!targetInv.resources.TryGetValue(resourceId, out ResourceState targetState)) return false;

        // Check permissions, stock, and available space
        if (!sourceState.allowTaking || !targetState.allowStoring) return false;
        if (sourceState.current < amount) return false;

        int availableSpace = targetState.max - targetState.current;
        if (availableSpace < amount) return false;

        // Move resource between inventories
        sourceState.current -= amount;
        targetState.current += amount;

        // Notify changes
        OnResourceChanged?.Invoke(sourceId, resourceId, sourceState.current, sourceState.max);
        OnResourceChanged?.Invoke(targetId, resourceId, targetState.current, targetState.max);

        return true;
    }

    public Dictionary<int, ResourceState> GetReadOnlyInventory(int storageId)
    {
        if (activeStorages.TryGetValue(storageId, out InventoryData inv))
        {
            return inv.resources;
        }
        return null;
    }
}