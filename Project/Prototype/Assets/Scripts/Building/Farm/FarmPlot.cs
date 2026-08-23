using UnityEngine;

public class FarmPlot : InteractableComponent, IInjectable
{
    [Header("Farming Settings")]
    [SerializeField] private int requiredSeedId = 1;
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private Transform plantSpawnPoint;

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
    }

    private void Update()
    {
        if (plotCollider != null)
        {
            plotCollider.enabled = (currentPlant == null);
        }
    }

    protected override void ExecuteInteraction(GameObject interactor)
    {
        if (currentPlant != null || resourceHub == null) return;

        StorageComponent playerStorage = interactor.GetComponent<StorageComponent>();

        if (playerStorage != null)
        {
            // Check if player has requirements
            if (resourceHub.HasEnough(playerStorage.StorageID, requiredSeedId, 1))
            {
                // Consume seed
                resourceHub.ConsumeResource(playerStorage.StorageID, requiredSeedId, 1, true);

                // Spawn plant
                Vector3 spawnPos = plantSpawnPoint != null ? plantSpawnPoint.position : transform.position;
                currentPlant = Instantiate(plantPrefab, spawnPos, Quaternion.identity);
                currentPlant.transform.SetParent(transform);

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
