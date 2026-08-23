using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(DangerComponent))]
[RequireComponent(typeof(Melee))]
public abstract class BaseRobotAI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private RobotStatsData stats;
    [SerializeField] private LayerMask enemyLayer;

    [SerializeField] private LeaderRobot assignedLeader;
    [HideInInspector][SerializeField] private Transform currentTarget;
    [HideInInspector][SerializeField] private Vector3 tacticalWaypoint;
    [HideInInspector][SerializeField] private bool holdAttack = false;
    [HideInInspector][SerializeField] private float lastAttackTime = 0f;

    [Header("Lifecycle")]
    [SerializeField] private float maxIdleTime = 120f;
    [SerializeField] private float currentIdleTime = 0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugVisuals = true;

    private float scanTimer = 0f;

    // Components
    public IRobotMover Mover { get; private set; }
    public HealthComponent Health { get; private set; }
    public DangerComponent Danger { get; private set; }
    public Melee MeleeWeapon { get; private set; }

    // State
    public IRobotState CurrentState { get; protected set; }
    public IRobotState Idle { get; protected set; }
    public IRobotState Wander { get; protected set; }
    public IRobotState Attack { get; protected set; }
    public IRobotState Despawn { get; protected set; }

    // Getters and Setters
    public RobotStatsData Stats => stats;
    public Vector3 TacticalWaypoint { get => tacticalWaypoint; set => tacticalWaypoint = value; }
    public LeaderRobot AssignedLeader { get => assignedLeader; set => assignedLeader = value; }
    public Transform CurrentTarget { get => currentTarget; set => currentTarget = value; }
    public bool HoldAttack { get => holdAttack; set => holdAttack = value; }
    public float LastAttackTime { get => lastAttackTime; set => lastAttackTime = value; }

    public EnemyHandler WaveHandler { get; set; }

    protected virtual void Awake()
    {
        Mover = GetComponent<IRobotMover>();
        Health = GetComponent<HealthComponent>();
        Danger = GetComponent<DangerComponent>();
        MeleeWeapon = GetComponent<Melee>();

        Despawn = new DespawnState();
    }


    protected virtual void Start()
    {
        // Apply stats
        if (stats != null)
        {
            Health.Initialize(stats.MaxHealth);
            Mover.SetSpeed(stats.MoveSpeed);
        }

        InitializeRobot();
    }

    protected virtual void Update()
    {
        if (Health.IsDead) return;

        if (CurrentState == Attack)
        {
            currentIdleTime = 0f;
        }
        else if (CurrentState != Despawn)
        {
            currentIdleTime += Time.deltaTime;
            if (currentIdleTime >= maxIdleTime)
            {
                ChangeState(Despawn);
                return;
            }
        }

        // Target scanning
        scanTimer += Time.deltaTime;
        if (scanTimer >= 0.5f)
        {
            scanTimer = 0f;
            Transform spotted = GetBestTargetLocally();
            if (spotted != null) ReportTarget(spotted);

            if (assignedLeader == null)
            {
                LookForHiveMind();
            }
        }

        CurrentState?.UpdateState(this);
    }

    public void ChangeState(IRobotState newState)
    {
        CurrentState?.ExitState(this);
        CurrentState = newState;
        CurrentState?.EnterState(this);
    }

    // Scan for closest enemy
    public virtual Transform GetBestTargetLocally()
    {
        if (stats == null) return null;

        // Ignore if isn't aggressive
        if (WaveHandler != null && WaveHandler.GlobalAggression < 5) return null;

        Collider[] enemies = Physics.OverlapSphere(transform.position, stats.DetectionRadius, enemyLayer);
        if (enemies.Length == 0) return null;

        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in enemies)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = col.transform;
            }
        }
        return bestTarget;
    }


    public virtual void ReportTarget(Transform target)
    {
        if (target == null) return;

        // Report spotted targets to leader
        if (assignedLeader != null)
        {
            assignedLeader.ReportTarget(target);
            return;
        }

        // If no leader, touch him
        if (currentTarget == null || CurrentState != Attack)
        {
            currentTarget = target;
            if (CurrentState != Attack) ChangeState(Attack);
        }
    }

    // React when taking damage
    public void AlertUnderAttack(Transform attacker)
    {
        if (attacker != null)
        {
            // Increase aggression
            if (WaveHandler != null) WaveHandler.IncreaseAggression(2);
            ReportTarget(attacker);
        }
    }

    // Find closest point range checks
    public Vector3 GetTargetEdge()
    {
        if (currentTarget == null) return transform.position;

        Collider targetCol = currentTarget.GetComponentInChildren<Collider>();
        if (targetCol != null)
        {
            return targetCol.ClosestPoint(transform.position);
        }

        return currentTarget.position;
    }

    private void LookForHiveMind()
    {
        // Leader can't join another leader ofc
        if (this is LeaderRobot) return;

        Collider[] allies = Physics.OverlapSphere(transform.position, stats.DetectionRadius, 1 << gameObject.layer);

        foreach (Collider col in allies)
        {
            if (col.gameObject == this.gameObject) continue;

            BaseRobotAI friend = col.GetComponentInParent<BaseRobotAI>();
            if (friend == null || friend.Health.IsDead) continue;

            LeaderRobot foundLeader = null;

            if (friend is LeaderRobot)
            {
                foundLeader = (LeaderRobot)friend;
            }
            // Check if robot is already assigned
            else if (friend.assignedLeader != null)
            {
                foundLeader = friend.assignedLeader;
            }

            if (foundLeader != null)
            {
                // Joining the squaddd
                assignedLeader = foundLeader;

                if (!assignedLeader.Squad.Contains(this))
                {
                    assignedLeader.Squad.Add(this);
                }
                break;
            }
        }
    }

    public void PerformMeleeAttack(int damageAmount)
    {
        if (MeleeWeapon != null)
        {
            MeleeWeapon.PerformAttack(damageAmount, enemyLayer);
        }
    }

    protected abstract void InitializeRobot();

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugVisuals) return;

        // Show detection range
        Gizmos.color = (CurrentState != null && CurrentState.GetType() == typeof(AttackState)) ? Color.red : Color.yellow;
        float radius = stats != null ? stats.DetectionRadius : 15f;
        Gizmos.DrawWireSphere(transform.position, radius);

        // Show assigned waypoint
        if (tacticalWaypoint != Vector3.zero && holdAttack)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, tacticalWaypoint);
            Gizmos.DrawWireSphere(tacticalWaypoint, 1f);
        }

        // Show current state
        if (CurrentState != null)
        {
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, CurrentState.GetType().Name);
        }
    }
#endif
}