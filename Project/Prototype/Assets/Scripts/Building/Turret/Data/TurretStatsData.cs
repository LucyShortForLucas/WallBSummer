using UnityEngine;

[CreateAssetMenu(fileName = "TurretStats", menuName = "Building/Turret/Turret Stats")]
public class TurretStatsData : ScriptableObject
{
    public enum TargetPriority { Closest, LowestHealth }

    [Header("Targeting")]
    [SerializeField] private TargetPriority priority = TargetPriority.Closest;
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float minRange = 2f;

    [Header("Turret Behaviour")]
    [SerializeField] private int maxHealth = 250;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float turnSpeed = 8f;

    [SerializeField] private float maxDepression = -15f;
    [SerializeField] private float maxElevation = 45f;

    // Getters
    public TargetPriority Priority { get => priority; }
    public float DetectionRadius { get => detectionRadius; }
    public float MinRange { get => minRange; }
    public int MaxHealth { get => maxHealth; }
    public float FireRate { get => fireRate; }
    public float TurnSpeed { get => turnSpeed; }
    public float MaxDepression { get => maxDepression; }
    public float MaxElevation { get => maxElevation; }
}