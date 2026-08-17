using UnityEngine;

[CreateAssetMenu(fileName = "RobotStats", menuName = "Entity/Robot/Robot Stats")]
public class RobotStatsData : ScriptableObject
{
    [Header("Core")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float detectionRadius = 15f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 10;

    // Getters and Setters
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float DetectionRadius { get => detectionRadius; set => detectionRadius = value; }
    public float AttackRange { get => attackRange; set => attackRange = value; }
    public float AttackCooldown { get => attackCooldown; set => attackCooldown = value; }
    public int AttackDamage { get => attackDamage; set => attackDamage = value; }
}