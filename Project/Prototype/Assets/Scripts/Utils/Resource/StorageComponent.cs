using System;
using System.Collections.Generic;
using UnityEngine;

public class StorageComponent : MonoBehaviour
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

    private void Awake()
    {
        // Get unique ID from hub
        StorageID = GameManager.ResourceHub.RegisterStorage(isPlayerOwned);

        // Configure starting inventory slots
        foreach (var slot in initialSlots)
        {
            GameManager.ResourceHub.SetupResourceSlot(
                StorageID,
                slot.resourceId,
                slot.maxCapacity,
                slot.allowStoring,
                slot.allowTaking
            );

            // Add initial starting loot
            if (slot.startingAmount > 0)
            {
                GameManager.ResourceHub.AddResource(StorageID, slot.resourceId, slot.startingAmount);
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.ResourceHub != null)
        {
            GameManager.ResourceHub.UnregisterStorage(StorageID);
        }
    }
}