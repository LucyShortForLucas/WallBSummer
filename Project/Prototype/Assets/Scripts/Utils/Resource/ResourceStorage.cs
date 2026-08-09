using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceStorage : MonoBehaviour, IResourceContainer
{
    public event Action<ResourceData, int, int> OnResourceChanged;

    [Serializable]
    public class ResourceSlot
    {
        public ResourceData data;
        public int amount;
        public int maxCapacity = 100;

        [Header("Resource Permissions")]
        public bool allowStoring = true;
        public bool allowTaking = true;
    }

    // List of all the resources
    [SerializeField] private List<ResourceSlot> startingResources = new List<ResourceSlot>();

    private class ResourceState
    {
        public int current;
        public int max;
        public bool allowStoring;
        public bool allowTaking;
    }

    // Dictionary to hold the current state of each resource
    private Dictionary<ResourceData, ResourceState> resourceDict = new Dictionary<ResourceData, ResourceState>();

    private void Awake()
    {
        foreach (var slot in startingResources)
        {
            // Maps each resource asset to state object
            resourceDict[slot.data] = new ResourceState
            {
                current = slot.amount,
                max = slot.maxCapacity,
                allowStoring = slot.allowStoring,
                allowTaking = slot.allowTaking
            };
        }
    }

    public void AddResource(ResourceData resource, int amount)
    {
        if (!resourceDict.ContainsKey(resource))
        {
            resourceDict[resource] = new ResourceState { current = 0, max = 999, allowStoring = true, allowTaking = true };
        }

        ResourceState data = resourceDict[resource];
        data.current += amount;
        data.current = Mathf.Min(data.current, data.max);

        OnResourceChanged?.Invoke(resource, data.current, data.max);
    }

    public bool ConsumeResource(ResourceData resource, int amount)
    {
        if (HasEnough(resource, amount))
        {
            ResourceState data = resourceDict[resource];
            data.current -= amount;
            OnResourceChanged?.Invoke(resource, data.current, data.max);
            return true;
        }
        return false;
    }

    public bool HasEnough(ResourceData resource, int amount)
    {
        return resourceDict.TryGetValue(resource, out ResourceState data) && data.current >= amount;
    }

    public int GetAmount(ResourceData resource)
    {
        return resourceDict.TryGetValue(resource, out ResourceState data) ? data.current : 0;
    }

    public int GetMaxAmount(ResourceData resource)
    {
        return resourceDict.TryGetValue(resource, out ResourceState data) ? data.max : 0;
    }

    public IEnumerable<ResourceData> GetAllTrackedResources()
    {
        return resourceDict.Keys;
    }

    public bool CanStore(ResourceData resource)
    {
        return resourceDict.TryGetValue(resource, out ResourceState data) && data.allowStoring;
    }

    public bool CanTake(ResourceData resource)
    {
        return resourceDict.TryGetValue(resource, out ResourceState data) && data.allowTaking;
    }
}