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
    public int MaxHealth { get => maxHealth; }
    public float MoveSpeed { get => moveSpeed;  }
    public float DetectionRadius { get => detectionRadius;  }
    public float AttackRange { get => attackRange;  }
    public float AttackCooldown { get => attackCooldown;  }
    public int AttackDamage { get => attackDamage;  }
}