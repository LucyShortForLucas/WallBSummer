using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(DangerComponent))]
public abstract class BaseTurret : MonoBehaviour
{
    public TurretStatsData stats;                // VIOLATES Cs.S.1 - Avoid declaring public fields in a class
    public ProjectileStatsData projectileStats;  // ^^^ 
    public LayerMask enemyLayer;                 // ^^^ 
                                                 // ^^^ 
    public Transform rotator;                    // ^^^ 
    public Transform barrel;                     // ^^^ 
    public Transform firePoint;                  // ^^^ 
    public GameObject projectilePrefab;          // ^^^ 

    public HealthComponent Health { get; private set; } // VIOLATES Cs.S.2 - Avoid auto-implemented properties.
    public DangerComponent Danger { get; private set; } // ^^

    protected Transform currentTarget;
    protected float nextFireTime;

    protected bool needsNewTarget = true;

    protected virtual void Awake()
    {
        Health = GetComponent<HealthComponent>();
        Danger = GetComponent<DangerComponent>();
    }

    protected virtual void Start()
    {
        if (stats != null) Health.Initialize(stats.maxHealth);
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

        if (distance > stats.detectionRadius || distance < stats.minRange) return true;

        return false;
    }

    private void FindTarget()
    {
        if (stats == null) return;

        Collider[] enemies = Physics.OverlapSphere(transform.position, stats.detectionRadius, enemyLayer);

        Transform bestTarget = null;
        float bestValue = Mathf.Infinity;

        foreach (Collider col in enemies)
        {
            HealthComponent targetHealth = col.GetComponentInParent<HealthComponent>();
            if (targetHealth != null && targetHealth.IsDead) continue;

            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance < stats.minRange) continue;

            if (stats.priority == TurretStatsData.TargetPriority.Closest)
            {
                if (distance < bestValue)
                {
                    bestValue = distance;
                    bestTarget = col.transform;
                }
            }
            else if (stats.priority == TurretStatsData.TargetPriority.LowestHealth)
            {
                if (targetHealth.currentHealth < bestValue)
                {
                    bestValue = targetHealth.currentHealth;
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
                rotator.rotation = Quaternion.Slerp(rotator.rotation, targetBaseRot, stats.turnSpeed * Time.deltaTime);
            }
        }

        float groundDistance = groundDir.magnitude;
        float heightDifference = currentTarget.position.y - barrel.position.y;

        float pitchAngle = Mathf.Atan2(heightDifference, groundDistance) * Mathf.Rad2Deg;
        pitchAngle = Mathf.Clamp(pitchAngle, stats.maxDepression, stats.maxElevation);

        Quaternion targetBarrelRot = Quaternion.Euler(-pitchAngle, 0, 0);

        if (Quaternion.Angle(barrel.localRotation, targetBarrelRot) > 0.1f)
        {
            barrel.localRotation = Quaternion.Slerp(barrel.localRotation, targetBarrelRot, stats.turnSpeed * Time.deltaTime);
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
        Gizmos.DrawWireSphere(transform.position, stats.detectionRadius);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, stats.minRange);

        if (currentTarget != null && firePoint != null)
        {
            Gizmos.DrawLine(firePoint.position, currentTarget.position);
        }
    }
#endif
}