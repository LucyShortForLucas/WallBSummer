using System.Collections.Generic;
using UnityEngine;

public class WaterComponent : MonoBehaviour
{
    [Header("Water Settings")]
    [SerializeField] private float waterCapacity = 100f;
    [SerializeField] private float waterGenerator = 0f;

    [Header("Transform")]
    [SerializeField] private Transform waterReceiver;
    [SerializeField] private List<WaterComponent> connectedNodes = new List<WaterComponent>();

    [Header("Live Data")]
    [SerializeField] private float currentWater = 0f;

    private float hiddenTransferRate = 25f;

    // Getters and Setters
    public List<WaterComponent> ConnectedNodes { get => connectedNodes; }
    public Transform WaterReceiver { get => waterReceiver; }
    public float CurrentWater { get => currentWater; set => currentWater = Mathf.Clamp(value, 0f, waterCapacity); }
    public float WaterCapacity { get => waterCapacity; set => waterCapacity = Mathf.Max(0f, value); }


    private void Awake()
    {
        if (waterReceiver == null) waterReceiver = transform;
    }

    private void Update()
    {
        // Generate Water
        if (waterGenerator > 0)
        {
            currentWater += waterGenerator * Time.deltaTime;
        }

        // Cap max
        currentWater = Mathf.Clamp(currentWater, 0f, waterCapacity);

        // Transfer water
        foreach (var connectedBuilding in connectedNodes)
        {
            // If not less water than connected building
            if (this.currentWater > connectedBuilding.currentWater)
            {
                float difference = this.currentWater - connectedBuilding.currentWater;

                float transferAmount = Mathf.Min(hiddenTransferRate * Time.deltaTime, difference / 2f);

                // Cap max on receiving building
                if (connectedBuilding.currentWater + transferAmount > connectedBuilding.waterCapacity)
                {
                    transferAmount = connectedBuilding.waterCapacity - connectedBuilding.currentWater;
                }

                // Transfer water
                this.currentWater -= transferAmount;
                connectedBuilding.currentWater += transferAmount;
            }
        }
    }
}