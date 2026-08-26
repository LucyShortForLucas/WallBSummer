using UnityEngine;
using System;

public class FarmPlot : InteractableComponent, IInjectable
{
    [Header("Farming Settings")]
    [SerializeField] private int requiredSeedId = 1;
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private Transform plantSpawnPoint;

    [Header("Water")]
    [SerializeField] private WaterComponent waterComponent;
    [SerializeField] private Renderer plotRenderer;
    [SerializeField] private Color dryColor = new Color(0.6f, 0.4f, 0.2f);       
    [SerializeField] private Color wateredColor = new Color(0.3f, 0.15f, 0.05f);

    public event Action OnPlantEvent;

    private GameObject currentPlant;
    private Collider plotCollider;
    private CentralResourceHub resourceHub;

    public void Inject(DependencyContainer container)
    {
        resourceHub = container.Get<CentralResourceHub>();
    }

    protected override void Awake()
    {
        base.Awake();
        plotCollider = GetComponent<Collider>();
        waterComponent = GetComponent<WaterComponent>();
        plotRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (plotCollider != null)
        {
            plotCollider.enabled = (currentPlant == null);
        }

        if (plotRenderer != null && waterComponent != null)
        {
            plotRenderer.material.color = waterComponent.CurrentWater >= 1f ? wateredColor : dryColor;
        }
    }

    protected override void ExecuteInteraction(GameObject interactor)
    {
        if (currentPlant != null || resourceHub == null) return;

        StorageComponent playerStorage = interactor.GetComponent<StorageComponent>();

        if (playerStorage != null)
        {
            if (waterComponent == null || waterComponent.CurrentWater < 1f)
            {
                return;
            }

            // Check if player has requirements
            if (resourceHub.HasEnough(playerStorage.StorageID, requiredSeedId, 1))
            {
                // Consume water
                waterComponent.CurrentWater -= 1f;

                // Consume seed
                resourceHub.ConsumeResource(playerStorage.StorageID, requiredSeedId, 1, true);

                // Spawn plant
                Vector3 spawnPos = plantSpawnPoint != null ? plantSpawnPoint.position : transform.position;
                currentPlant = Instantiate(plantPrefab, spawnPos, Quaternion.identity);
                currentPlant.transform.SetParent(transform);

                OnPlantEvent?.Invoke();

                // Pass the Hub down to the newly spawned plant!
                var plantScript = currentPlant.GetComponent<Plant>();
                if (plantScript != null)
                {
                    plantScript.Setup(resourceHub);
                }
            }
            else
            {
                Debug.Log("Not enough seeds to plant");
            }
        }
    }
}
