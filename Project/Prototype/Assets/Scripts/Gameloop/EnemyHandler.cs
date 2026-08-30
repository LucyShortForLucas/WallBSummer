using System.Collections.Generic;
using UnityEngine;

public class EnemyHandler : MonoBehaviour, IInjectable
{
    [Header("Wave State")]
    private int nextWaveDay = 0;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject leaderPrefab;
    [SerializeField] private GameObject minionPrefab;

    [Header("Spawning Rules")]
    [SerializeField] private float spawnMapRadius = 100f;
    [SerializeField] private LayerMask alliesLayer;
    [SerializeField] private float safeDistance = 30f;
    [SerializeField] private int leaderSpawnStartDay = 5;

    [Header("Hive Memory")]
    [SerializeField] private int globalAggression = 0;
    private List<Vector3> fallenLeaderPositions = new List<Vector3>();
    private readonly int aggressionThreshold = 5;

    private TimeManager timeManager;

    // Getters and Setters
    public int GlobalAggression { get => globalAggression; }

    public void Inject(DependencyContainer container)
    {
        timeManager = container.Get<TimeManager>();

        timeManager.OnDayAdvanced += HandleDayAdvanced;

        nextWaveDay = Random.Range(2, 5);
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
        if (currentDay <= 1) return;

        if (currentDay >= nextWaveDay)
        {
            TriggerWave(currentDay);
            nextWaveDay = currentDay + Random.Range(1, 4);
        }
    }

    public void TriggerWave(int currentDay)
    {
        // Calculate groups based on days
        int spawnGroups = 1 + (currentDay / 10);

        // Check if long enough for leader
        int leadersToSpawn = (currentDay >= leaderSpawnStartDay) ? 1 + (currentDay / 30) : 0;

        // More minionsss
        int minionsPerGroup = Mathf.Min(10, 1 + (currentDay / 5));

        Debug.Log($"Wave triggered Day {currentDay}: Spawning {spawnGroups} groups ({leadersToSpawn} leaders total, {minionsPerGroup} minions per group).");

        Vector3? revengePos = GetRevengeTarget();

        // Spawn loops
        for (int i = 0; i < spawnGroups; i++)
        {
            Vector3 spawnPos = FindSafeSpawnPosition();

            // Spawn leader if available
            if (leadersToSpawn > 0)
            {
                GameObject leaderObj = Instantiate(leaderPrefab, spawnPos, Quaternion.identity);
                leaderObj.GetComponent<LeaderRobot>().SetupMemory(this, revengePos);
                leadersToSpawn--;
            }

            // Spawn minions
            for (int m = 0; m < minionsPerGroup; m++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * 5f;
                Vector3 minionPos = spawnPos + new Vector3(randomCircle.x, 0, randomCircle.y);

                GameObject minionObj = Instantiate(minionPrefab, minionPos, Quaternion.identity);
                minionObj.GetComponent<BaseRobotAI>().WaveHandler = this;
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

    public void IncreaseAggression(int amount)
    {
        globalAggression += amount;
        Debug.Log($"Aggression increased to {globalAggression}");
    }

    public void RecordLeaderDeath(Vector3 deathPosition)
    {
        fallenLeaderPositions.Add(deathPosition);
    }

    public Vector3? GetRevengeTarget()
    {
        // 50% chance to investigate a past death site if one exists
        if (fallenLeaderPositions.Count > 0 && Random.value > 0.5f)
        {
            return fallenLeaderPositions[Random.Range(0, fallenLeaderPositions.Count)];
        }
        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (fallenLeaderPositions == null || fallenLeaderPositions.Count == 0) return;

        Gizmos.color = Color.red;
        foreach (Vector3 pos in fallenLeaderPositions)
        {
            Gizmos.DrawSphere(pos, 1.5f);
            Gizmos.DrawLine(pos - Vector3.up * 2, pos + Vector3.up * 2);
            Gizmos.DrawLine(pos - Vector3.right * 2, pos + Vector3.right * 2);
            Gizmos.DrawLine(pos - Vector3.forward * 2, pos + Vector3.forward * 2);
        }
    }
#endif
}