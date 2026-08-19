using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceDatabase", menuName = "Database/Resource Database")]
public class GlobalResourceDatabase : ScriptableObject
{
    [Serializable]
    public struct ResourceDefinition
    {
        public int id;
        public string resourceName;
        public Sprite icon;
    }

    [Header("Resources")]
    public List<ResourceDefinition> allResources = new List<ResourceDefinition>();

    // Fast lookup for UI at runtime
    private Dictionary<int, ResourceDefinition> resourceLookup = new Dictionary<int, ResourceDefinition>();

    public void Initialize()
    {
        resourceLookup.Clear();

        // Put all resources in other dictionary
        foreach (var res in allResources)
        {
            if (!resourceLookup.ContainsKey(res.id))
            {
                resourceLookup.Add(res.id, res);
            }
            else
            {
                Debug.LogError($"Duplicate Resource ID found in Database: {res.id}");
            }
        }
    }

    public ResourceDefinition GetResource(int id)
    {
        if (resourceLookup != null && resourceLookup.TryGetValue(id, out ResourceDefinition data))
        {
            return data;
        }
        return default;
    }
}