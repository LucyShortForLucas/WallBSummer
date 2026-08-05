using UnityEngine;

[CreateAssetMenu(fileName = "TurretStats", menuName = "Building/Turret/Turret Stats")]
public class TurretStatsData : ScriptableObject
{
    public enum TargetPriority { Closest, LowestHealth }

    [Header("Targeting")]
    public TargetPriority priority = TargetPriority.Closest;
    public float detectionRadius = 20f;
    public float minRange = 2f;

    [Header("Turret Behaviour")]
    public int maxHealth = 250;
    public float fireRate = 1.5f;
    public float turnSpeed = 8f;

    public float maxDepression = -15f;
    public float maxElevation = 45f;
}