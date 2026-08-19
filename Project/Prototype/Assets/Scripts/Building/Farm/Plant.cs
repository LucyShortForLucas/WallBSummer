using UnityEngine;

public class Plant : InteractableComponent
{
    public enum PlantState
    {
        Growing,
        Mature,
        Expired
    }

    [Header("Growth Timers")]
    [SerializeField] private float timeToMature = 30f;
    [SerializeField] private float timeToExpire = 60f;
    [SerializeField] private float timeToDestroy = 90f;

    [Header("Resource Definitions")]
    [SerializeField] private int seedResourceId = 1;
    [SerializeField] private int foodResourceId = 2;

    [Header("Yield Amounts")]
    [SerializeField] private int matureSeedYield = 1;
    [SerializeField] private int matureFoodYield = 2;

    [Header("Visuals")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color growingColor = new Color(0.5f, 1f, 0.5f);
    [SerializeField] private Color matureColor = new Color(0f, 0.5f, 0f);
    [SerializeField] private Color expiredColor = new Color(0.4f, 0.3f, 0.1f);

    private PlantState currentState;
    private float ageTimer = 0f;
    private CentralResourceHub resourceHub;

    public void Setup(CentralResourceHub hub)
    {
        resourceHub = hub;
    }

    protected override void Awake()
    {
        base.Awake();

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        SetState(PlantState.Growing);
    }

    private void Update()
    {
        ageTimer += Time.deltaTime;

        if (currentState == PlantState.Growing && ageTimer >= timeToMature)
        {
            SetState(PlantState.Mature);
        }
        else if (currentState == PlantState.Mature && ageTimer >= timeToExpire)
        {
            SetState(PlantState.Expired);
        }
        else if (currentState == PlantState.Expired && ageTimer >= timeToDestroy)
        {
            Destroy(gameObject);
        }
    }

    private void SetState(PlantState newState)
    {
        currentState = newState;

        if (targetRenderer != null)
        {
            switch (currentState)
            {
                case PlantState.Growing:
                    targetRenderer.material.color = growingColor;
                    break;
                case PlantState.Mature:
                    targetRenderer.material.color = matureColor;
                    break;
                case PlantState.Expired:
                    targetRenderer.material.color = expiredColor;
                    break;
            }
        }
    }

    protected override void ExecuteInteraction(GameObject interactor)
    {
        if (resourceHub == null) return;

        StorageComponent playerStorage = interactor.GetComponent<StorageComponent>();

        if (playerStorage != null)
        {
            if (currentState == PlantState.Mature)
            {
                // Perfect, 1 Seed and 2 Food
                resourceHub.AddResource(playerStorage.StorageID, seedResourceId, matureSeedYield, true);
                resourceHub.AddResource(playerStorage.StorageID, foodResourceId, matureFoodYield, true);
            }
            else
            {
                // Wrong, 1 Seed
                resourceHub.AddResource(playerStorage.StorageID, seedResourceId, 1, true);
            }
        }

        Destroy(gameObject);
    }
}