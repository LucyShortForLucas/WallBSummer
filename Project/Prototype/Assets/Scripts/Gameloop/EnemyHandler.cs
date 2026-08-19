using UnityEngine;

public class EnemyHandler : MonoBehaviour, IInjectable
{
    [Header("Wave State")]
    private int daysUntilNextWave = 0;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject leaderPrefab;
    [SerializeField] private GameObject minionPrefab;

    [Header("Spawning Rules")]
    [SerializeField] private float spawnMapRadius = 100f;
    [SerializeField] private LayerMask alliesLayer;
    [SerializeField] private float safeDistance = 30f;

    private TimeManager timeManager;

    public void Inject(DependencyContainer container)
    {
        timeManager = container.Get<TimeManager>();

        timeManager.OnDayAdvanced += HandleDayAdvanced;

        daysUntilNextWave = 0;

        if (daysUntilNextWave <= 0)
        {
            TriggerWave(timeManager.currentDay);
            daysUntilNextWave = Random.Range(1, 4);
        }
    }

    private void OnDestroy()
    {
        if (timeManager != null)
        {
            timeManager.OnDayAdvanced -= HandleDayAdvanced;
        }
    }

    private void HandleDayAdvanced(int currentDay)
    {
        daysUntilNextWave--;

        if (daysUntilNextWave <= 0)
        {
            TriggerWave(currentDay);
            daysUntilNextWave = Random.Range(1, 4);
        }
    }

    private void TriggerWave(int currentDay)
    {
        int leaderCount = 1 + (currentDay / 30);
        int minionsPerLeader = Mathf.Min(10, 2 + (currentDay / 10));

        Debug.Log($"Wave timeeeeeee, Spawning {leaderCount} Leaders with {minionsPerLeader} minions each");

        for (int i = 0; i < leaderCount; i++)
        {
            Vector3 spawnPos = FindSafeSpawnPosition();
            Instantiate(leaderPrefab, spawnPos, Quaternion.identity);

            for (int m = 0; m < minionsPerLeader; m++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * 5f;
                Vector3 minionPos = spawnPos + new Vector3(randomCircle.x, 0, randomCircle.y);

                Instantiate(minionPrefab, minionPos, Quaternion.identity);
            }
        }
    }

    private Vector3 FindSafeSpawnPosition()
    {
        int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * spawnMapRadius;
            Vector3 testPos = new Vector3(randomPoint.x, 0, randomPoint.y);

            if (!Physics.CheckSphere(testPos, safeDistance, alliesLayer))
            {
                return testPos;
            }
        }

        return new Vector3(spawnMapRadius, 0, 0);
    }
}