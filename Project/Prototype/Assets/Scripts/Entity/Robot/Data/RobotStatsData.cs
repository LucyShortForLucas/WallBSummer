using UnityEngine;

[CreateAssetMenu(fileName = "RobotStats", menuName = "Entity/Robot/Robot Stats")]
public class RobotStatsData : ScriptableObject
{
    [Header("Core")]
    public int maxHealth = 100;
    public float moveSpeed = 5f;
    public float detectionRadius = 15f;

    [Header("Combat")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;
}