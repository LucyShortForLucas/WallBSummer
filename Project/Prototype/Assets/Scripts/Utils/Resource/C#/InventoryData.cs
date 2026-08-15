using System.Collections.Generic;
using UnityEngine;

public class InventoryData
{
    public int storageId;
    public bool isPlayerOwned;

    // Key: Resource ID, Value: state of resource
    public Dictionary<int, ResourceState> resources = new Dictionary<int, ResourceState>();

    public InventoryData(int id, bool playerOwned)
    {
        storageId = id;
        isPlayerOwned = playerOwned;
    }
}