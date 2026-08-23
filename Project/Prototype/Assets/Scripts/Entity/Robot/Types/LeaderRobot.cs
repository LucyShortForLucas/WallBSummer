using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(GroundMover))]
[RequireComponent(typeof(TargetAssigner))]
[RequireComponent(typeof(FormationDirector))]
public class LeaderRobot : BaseRobotAI
{
    public enum TacticalStance { Offensive, Neutral, Defensive }

    [Header("Tactics Playbook")]
    [SerializeField] private StrategyProfile strategyProfile;
    [SerializeField] private List<BaseRobotAI> squad = new List<BaseRobotAI>();

    [Header("Dynamic Strategy")]
    [SerializeField] private TacticalStance currentStance = TacticalStance.Neutral;
    [SerializeField] private float currentWinrate = 1f;
    [SerializeField] private float lowHealthRetreatThreshold = 0.3f;

    [SerializeField] private FormationData activeFormation;
    [SerializeField] private HashSet<Transform> knownThreats = new HashSet<Transform>();
    [SerializeField] private float smoothedCommandDistance = -1f;

    [SerializeField] private TargetAssigner assigner;
    [SerializeField] private FormationDirector director;

    private bool isRegrouping = false;
    private Vector3? revengeTarget;

    // Getters and setters
    public List<BaseRobotAI> Squad => squad;

    protected override void Awake()
    {
        base.Awake();
        assigner = GetComponent<TargetAssigner>();
        director = GetComponent<FormationDirector>();
    }

    protected override void InitializeRobot()
    {
        Idle = new IdleState();
        Wander = new WanderState();
        Attack = new AttackState();
        ChangeState(Idle);
    }

    public void SetupMemory(EnemyHandler handler, Vector3? revengePos)
    {
        WaveHandler = handler;
        revengeTarget = revengePos;
    }

    public override void ReportTarget(Transform target)
    {
        bool isNewTarget = target != null && !knownThreats.Contains(target);
        base.ReportTarget(target);

        if (target != null) knownThreats.Add(target);

        // If finds the base/player, trigger regroup
        if (isNewTarget && CurrentState != Attack)
        {
            isRegrouping = true;
            Invoke(nameof(EndRegroup), 5f); // Wait 5 seconds
        }
    }

    private void EndRegroup()
    {
        isRegrouping = false;
    }

    protected override void Update()
    {
        if (Health.IsDead)
        {
            if (WaveHandler != null) WaveHandler.RecordLeaderDeath(transform.position);
            return;
        }

        // If no targets, check if revenge waypoint to wander towards
        if (CurrentTarget == null && revengeTarget.HasValue && CurrentState == Wander)
        {
            Mover.MoveTo(revengeTarget.Value);
        }

        base.Update();
        if (CurrentState == Attack && knownThreats.Count > 0)
        {
            ExecuteStrategy();
        }
    }

    private void ExecuteStrategy()
    {
        if (strategyProfile == null) return;

        // Prune dead squad members
        squad.RemoveAll(bot => bot == null || bot.Health.IsDead);
        if (squad.Count == 0) return;

        // Analyze Field
        CalculateWinrateAndStance();
        Vector3 dangerCenter = GetDangerCenter(out List<Transform> validThreats);

        // Decide Stance & Move Leader
        bool isEscortProtocolActive = ((float)Health.CurrentHealth / Stats.MaxHealth) <= lowHealthRetreatThreshold && currentStance == TacticalStance.Offensive;
        UpdateLeaderWaypoint(dangerCenter, isEscortProtocolActive);

        // Sort & Split Squad
        validThreats.Sort((a, b) =>
        {
            float d1 = a.GetComponent<DangerComponent>() != null ? a.GetComponent<DangerComponent>().CurrentDanger : 0f;
            float d2 = b.GetComponent<DangerComponent>() != null ? b.GetComponent<DangerComponent>().CurrentDanger : 0f;
            int compare = d2.CompareTo(d1);
            return compare == 0 ? a.gameObject.GetEntityId().CompareTo(b.gameObject.GetEntityId()) : compare;
        });

        squad.Sort((a, b) => a.gameObject.GetEntityId().CompareTo(b.gameObject.GetEntityId()));
        List<BaseRobotAI> attackers = new List<BaseRobotAI>();
        List<BaseRobotAI> escorts = new List<BaseRobotAI>();

        // Separate escorts from attackers
        foreach (BaseRobotAI bot in squad)
        {
            if (isEscortProtocolActive && escorts.Count < 2) escorts.Add(bot);
            else attackers.Add(bot);
        }

        // Assign Targets
        assigner.AssignEscorts(escorts, transform, CurrentTarget);

        if (currentStance == TacticalStance.Offensive)
            assigner.AssignProportionalTargets(attackers, validThreats, CurrentTarget);
        else
            foreach (var bot in attackers) bot.CurrentTarget = CurrentTarget;

        // Enforce Engagement Rules
        List<BaseRobotAI> loyalBots = new List<BaseRobotAI>();

        // If Offensive and close to danger, drop formation and unleash the brawlers
        bool isBrawling = !isRegrouping && currentStance == TacticalStance.Offensive && Vector3.Distance(transform.position, dangerCenter) < (activeFormation.SafeCommandDistance * 1.5f);

        foreach (BaseRobotAI bot in attackers)
        {
            if (bot.CurrentState != bot.Attack) bot.ChangeState(bot.Attack);

            if (isRegrouping)
            {
                // Force them back to leader
                bot.HoldAttack = true;
                loyalBots.Add(bot);
            }
            if (isBrawling)
            {
                // Swarm target freely
                bot.HoldAttack = false;
                bot.Mover.SetSpeed(bot.Stats.MoveSpeed);
            }
            else
            {
                // Follow distance rules
                float distanceToLeader = Vector3.Distance(bot.transform.position, transform.position);
                float distanceToEnemy = Vector3.Distance(bot.transform.position, bot.GetTargetEdge());

                float activeBreakDistance = (currentStance == TacticalStance.Offensive) ? activeFormation.BreakFormationDistance : bot.Stats.AttackRange;
                float activeRecallDistance = (currentStance == TacticalStance.Offensive) ? activeFormation.RecallDistance : activeFormation.RecallDistance * 0.5f;

                if (distanceToLeader > activeRecallDistance)
                {
                    bot.HoldAttack = true;
                    loyalBots.Add(bot);
                }
                else if (distanceToEnemy <= activeBreakDistance)
                {
                    bot.HoldAttack = false;
                    bot.Mover.SetSpeed(bot.Stats.MoveSpeed);
                }
                else
                {
                    bot.HoldAttack = true;
                    loyalBots.Add(bot);
                }
            }
        }

        loyalBots.AddRange(escorts);

        // Shape the Grid
        if (loyalBots.Count > 0)
        {
            director.ApplyFormation(loyalBots, dangerCenter, activeFormation, transform);
            director.SynchronizeSpeeds(loyalBots, Stats.MoveSpeed, Mover, TacticalWaypoint, transform, activeFormation.BreakFormationDistance);
        }
    }

    private Vector3 GetDangerCenter(out List<Transform> validThreats)
    {
        validThreats = new List<Transform>();
        Vector3 center = Vector3.zero;

        // Filter valid targets and accumulate positions
        foreach (Transform t in knownThreats)
        {
            if (t != null && t.GetComponent<HealthComponent>() != null && !t.GetComponent<HealthComponent>().IsDead)
            {
                validThreats.Add(t);
                center += t.position;
            }
        }

        return validThreats.Count > 0 ? center / validThreats.Count : transform.position;
    }

    private void UpdateLeaderWaypoint(Vector3 dangerCenter, bool isEscortActive)
    {
        float targetCommandDistance = activeFormation.SafeCommandDistance;

        // Calculate target distance
        if (isRegrouping) targetCommandDistance = activeFormation.SafeCommandDistance * 3.5f;
        else if (isEscortActive) targetCommandDistance = activeFormation.SafeCommandDistance * 2.5f;
        else if (currentStance == TacticalStance.Offensive) targetCommandDistance = Stats.AttackRange * 0.8f;
        else if (currentStance == TacticalStance.Defensive) targetCommandDistance *= strategyProfile.DefensiveDistanceMultiplier;
        else if (currentStance == TacticalStance.Neutral) targetCommandDistance *= strategyProfile.NeutralDistanceMultiplier;

        if (smoothedCommandDistance < 0f) smoothedCommandDistance = targetCommandDistance;
        smoothedCommandDistance = Mathf.Lerp(smoothedCommandDistance, targetCommandDistance, Time.deltaTime * 0.5f);

        Vector3 dirToDanger = (dangerCenter - transform.position);
        dirToDanger.y = 0;
        if (dirToDanger.sqrMagnitude < 0.1f) dirToDanger = transform.forward;

        // Update waypoint and attack hold flag
        if (isRegrouping)
        {
            TacticalWaypoint = dangerCenter + (-dirToDanger.normalized * smoothedCommandDistance);
            HoldAttack = true;
        }
        else if (!isEscortActive && currentStance == TacticalStance.Offensive)
        {
            TacticalWaypoint = dangerCenter - (dirToDanger.normalized * smoothedCommandDistance);
            HoldAttack = false;
        }
        else
        {
            TacticalWaypoint = dangerCenter + (-dirToDanger.normalized * smoothedCommandDistance);
            HoldAttack = true;
        }
    }

    private void CalculateWinrateAndStance()
    {
        float squadPower = Danger.CurrentDanger;
        foreach (BaseRobotAI bot in squad) squadPower += bot.Danger.CurrentDanger;

        knownThreats.RemoveWhere(t => t == null || t.GetComponent<HealthComponent>() == null || t.GetComponent<HealthComponent>().IsDead);

        float enemyPower = 0f;
        Transform closestThreat = null;
        float closestDistance = Mathf.Infinity;

        // Compute power ratio
        foreach (Transform threat in knownThreats)
        {
            DangerComponent threatDanger = threat.GetComponent<DangerComponent>();
            if (threatDanger != null) enemyPower += threatDanger.CurrentDanger;

            float dist = Vector3.Distance(transform.position, threat.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestThreat = threat;
            }
        }

        if (closestThreat != null) CurrentTarget = closestThreat;

        if (enemyPower < 0.1f) enemyPower = 0.1f;
        currentWinrate = squadPower / enemyPower;

        TacticalStance newStance;
        if (currentWinrate > 0.8f) newStance = TacticalStance.Offensive;
        else if (currentWinrate > 0.4f) newStance = TacticalStance.Neutral;
        else newStance = TacticalStance.Defensive;

        if (newStance != currentStance || activeFormation == null)
        {
            currentStance = newStance;
            SelectRandomFormation();
        }
    }

    private void SelectRandomFormation()
    {
        List<FormationData> pool = null;

        // Select pool matching stance
        if (currentStance == TacticalStance.Offensive) pool = strategyProfile.OffensiveFormations;
        else if (currentStance == TacticalStance.Neutral) pool = strategyProfile.NeutralFormations;
        else if (currentStance == TacticalStance.Defensive) pool = strategyProfile.DefensiveFormations;

        if (pool != null && pool.Count > 0)
        {
            int randomIndex = Random.Range(0, pool.Count);
            activeFormation = pool[randomIndex];
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (squad == null || squad.Count == 0) return;

        // Draw squad links
        Gizmos.color = Color.green;
        foreach (BaseRobotAI bot in squad)
        {
            if (bot != null) Gizmos.DrawLine(transform.position, bot.transform.position);
        }

        // Draw threat links
        Gizmos.color = Color.magenta;
        foreach (Transform threat in knownThreats)
        {
            if (threat != null) Gizmos.DrawLine(transform.position + Vector3.up * 1f, threat.position + Vector3.up * 1f);
        }
    }
#endif
}