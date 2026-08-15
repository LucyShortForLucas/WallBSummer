using UnityEngine;

public class EnemyHandler : MonoBehaviour
{
    [Header("Wave State")]
    private int daysUntilNextWave = 0;

    [Header("Enemy Prefabs")]
    public GameObject leaderPrefab;
    public GameObject minionPrefab;

    [Header("Spawning Rules")]
    public float spawnMapRadius = 100f;
    public LayerMask alliesLayer;
    public float safeDistance = 30f;

    private void Start()
    {
        daysUntilNextWave = 0;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayAdvanced += HandleDayAdvanced;

            if (daysUntilNextWave <= 0)
            {
                TriggerWave(GameManager.Instance.currentDay);
                daysUntilNextWave = Random.Range(1, 4);
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDayAdvanced -= HandleDayAdvanced;
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, spawnMapRadius);
    }
#endif
}