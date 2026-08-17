using System;
using System.Collections.Generic;
using UnityEngine;

public class StorageComponent : MonoBehaviour, IInjectable
{
    [Header("Ownership")]
    [SerializeField] private bool isPlayerOwned = true;

    [Serializable]
    public struct ResourceSlotConfig
    {
        public int resourceId;
        public int startingAmount;
        public int maxCapacity;
        public bool allowStoring;
        public bool allowTaking;
    }

    [Header("Inventory Configuration")]
    [SerializeField] private List<ResourceSlotConfig> initialSlots = new List<ResourceSlotConfig>();

    public int StorageID { get; private set; }

    private CentralResourceHub resourceHub;

    public void Inject(DependencyContainer container)
    {
        resourceHub = container.Get<CentralResourceHub>();

        // Get unique ID from hub
        StorageID = resourceHub.RegisterStorage(isPlayerOwned);

        // Configure starting inventory slots
        foreach (var slot in initialSlots)
        {
            resourceHub.SetupResourceSlot(
                StorageID,
                slot.resourceId,
                slot.maxCapacity,
                slot.allowStoring,
                slot.allowTaking
            );

            // Add initial starting amount
            if (slot.startingAmount > 0)
            {
                resourceHub.AddResource(StorageID, slot.resourceId, slot.startingAmount);
            }
        }
    }

    private void OnDestroy()
    {
        if (resourceHub != null)
        {
            resourceHub.UnregisterStorage(StorageID);
        }
    }
}