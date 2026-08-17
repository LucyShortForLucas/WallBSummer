using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(DangerComponent))]
public abstract class BaseTurret : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TurretStatsData stats;                
    [SerializeField] private ProjectileStatsData projectileStats;  
    [SerializeField] private LayerMask enemyLayer;

    [Header("Transforms")]
    [SerializeField] private Transform rotator;                     
    [SerializeField] private Transform barrel;                      
    [SerializeField] private Transform firePoint;                   
    [SerializeField] private GameObject projectilePrefab;           

    public HealthComponent Health { get; private set; } // VIOLATES Cs.S.2 - Avoid auto-implemented properties.
    public DangerComponent Danger { get; private set; } // ^^

    protected Transform currentTarget;
    protected float nextFireTime;

    protected bool needsNewTarget = true;

    // Getters and Setters
    public TurretStatsData Stats { get => stats; set => stats = value; }
    public ProjectileStatsData ProjectileStats { get => projectileStats; set => projectileStats = value; }
    public Transform FirePoint { get => firePoint; set => firePoint = value; }
    public GameObject ProjectilePrefab { get => projectilePrefab; set => projectilePrefab = value; }


    protected virtual void Awake()
    {
        Health = GetComponent<HealthComponent>();
        Danger = GetComponent<DangerComponent>();
    }

    protected virtual void Start()
    {
        if (stats != null) Health.Initialize(stats.MaxHealth);
    }

    protected virtual void Update()
    {
        if (Health.IsDead) return;

        if (currentTarget == null || TargetIsInvalid() || needsNewTarget)
        {
            FindTarget();
            needsNewTarget = false;
        }

        if (currentTarget != null)
        {
            AimAtTarget();

            if (Time.time >= nextFireTime && IsAimed())
            {
                Fire();
                needsNewTarget = true;
            }
        }
    }

    private bool TargetIsInvalid()
    {
        if (currentTarget == null) return true;

        HealthComponent targetHealth = currentTarget.GetComponentInParent<HealthComponent>();
        if (targetHealth == null || targetHealth.IsDead) return true;

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance > stats.DetectionRadius || distance < stats.MinRange) return true;

        return false;
    }

    private void FindTarget()
    {
        if (stats == null) return;

        Collider[] enemies = Physics.OverlapSphere(transform.position, stats.DetectionRadius, enemyLayer);

        Transform bestTarget = null;
        float bestValue = Mathf.Infinity;

        foreach (Collider col in enemies)
        {
            HealthComponent targetHealth = col.GetComponentInParent<HealthComponent>();
            if (targetHealth != null && targetHealth.IsDead) continue;

            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance < stats.MinRange) continue;

            if (stats.Priority == TurretStatsData.TargetPriority.Closest)
            {
                if (distance < bestValue)
                {
                    bestValue = distance;
                    bestTarget = col.transform;
                }
            }
            else if (stats.Priority == TurretStatsData.TargetPriority.LowestHealth)
            {
                if (targetHealth.CurrentHealth < bestValue)
                {
                    bestValue = targetHealth.CurrentHealth;
                    bestTarget = col.transform;
                }
            }
        }

        currentTarget = bestTarget;
    }

    private void AimAtTarget()
    {
        Vector3 dirToTarget = currentTarget.position - rotator.position;

        Vector3 groundDir = dirToTarget;
        groundDir.y = 0;

        if (groundDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetBaseRot = Quaternion.LookRotation(groundDir);

            if (Quaternion.Angle(rotator.rotation, targetBaseRot) > 0.1f)
            {
                rotator.rotation = Quaternion.Slerp(rotator.rotation, targetBaseRot, stats.TurnSpeed * Time.deltaTime);
            }
        }

        float groundDistance = groundDir.magnitude;
        float heightDifference = currentTarget.position.y - barrel.position.y;

        float pitchAngle = Mathf.Atan2(heightDifference, groundDistance) * Mathf.Rad2Deg;
        pitchAngle = Mathf.Clamp(pitchAngle, stats.MaxDepression, stats.MaxElevation);

        Quaternion targetBarrelRot = Quaternion.Euler(-pitchAngle, 0, 0);

        if (Quaternion.Angle(barrel.localRotation, targetBarrelRot) > 0.1f)
        {
            barrel.localRotation = Quaternion.Slerp(barrel.localRotation, targetBarrelRot, stats.TurnSpeed * Time.deltaTime);
        }
    }

    protected bool IsAimed()
    {
        Vector3 dirToTarget = (currentTarget.position - firePoint.position).normalized;
        float dotProduct = Vector3.Dot(firePoint.forward, dirToTarget);
        return dotProduct > 0.95f;
    }

    protected abstract void Fire();

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (stats == null) return;

        Gizmos.color = currentTarget != null ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stats.DetectionRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, stats.MinRange);

        if (currentTarget != null && firePoint != null)
        {
            Gizmos.DrawLine(firePoint.position, currentTarget.position);
        }
    }
#endif
}