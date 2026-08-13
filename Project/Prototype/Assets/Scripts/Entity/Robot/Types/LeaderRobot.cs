using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(GroundMover))]
[RequireComponent(typeof(TargetAssigner))]
[RequireComponent(typeof(FormationDirector))]
public class LeaderRobot : BaseRobotAI
{
    public enum TacticalStance { Offensive, Neutral, Defensive }

    [Header("Tactics Playbook")]
    public StrategyProfile strategyProfile;
    public List<BaseRobotAI> squad = new List<BaseRobotAI>();

    [Header("Dynamic Strategy")]
    public TacticalStance currentStance = TacticalStance.Neutral;
    public float currentWinrate = 1f;
    public float lowHealthRetreatThreshold = 0.3f;

    private FormationData activeFormation;
    private HashSet<Transform> knownThreats = new HashSet<Transform>();
    private float smoothedCommandDistance = -1f;

    private TargetAssigner assigner;
    private FormationDirector director;

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

    public override void ReportTarget(Transform target)
    {
        base.ReportTarget(target);
        if (target != null) knownThreats.Add(target);
    }

    protected override void Update()
    {
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
        bool isEscortProtocolActive = ((float)Health.currentHealth / stats.maxHealth) <= lowHealthRetreatThreshold && currentStance == TacticalStance.Offensive;
        UpdateLeaderWaypoint(dangerCenter, isEscortProtocolActive);

        // Sort & Split Squad
        validThreats.Sort((a, b) =>
        {
            float d1 = a.GetComponent<DangerComponent>() != null ? a.GetComponent<DangerComponent>().CurrentDanger : 0f;
            float d2 = b.GetComponent<DangerComponent>() != null ? b.GetComponent<DangerComponent>().CurrentDanger : 0f;
            int compare = d2.CompareTo(d1);
            return compare == 0 ? a.gameObject.GetInstanceID().CompareTo(b.gameObject.GetInstanceID()) : compare;
        });

        squad.Sort((a, b) => a.gameObject.GetInstanceID().CompareTo(b.gameObject.GetInstanceID()));

        List<BaseRobotAI> attackers = new List<BaseRobotAI>();
        List<BaseRobotAI> escorts = new List<BaseRobotAI>();

        // Separate escorts from attackers
        foreach (BaseRobotAI bot in squad)
        {
            if (isEscortProtocolActive && escorts.Count < 2) escorts.Add(bot);
            else attackers.Add(bot);
        }

        // Assign Targets
        assigner.AssignEscorts(escorts, transform, currentTarget);

        if (currentStance == TacticalStance.Offensive)
            assigner.AssignProportionalTargets(attackers, validThreats, currentTarget);
        else
            foreach (var bot in attackers) bot.currentTarget = currentTarget;

        // Enforce Engagement Rules
        List<BaseRobotAI> loyalBots = new List<BaseRobotAI>();

        // If Offensive and close to danger, drop formation and unleash the brawlers
        bool isBrawling = currentStance == TacticalStance.Offensive && Vector3.Distance(transform.position, dangerCenter) < (activeFormation.safeCommandDistance * 1.5f);

        foreach (BaseRobotAI bot in attackers)
        {
            if (bot.CurrentState != bot.Attack) bot.ChangeState(bot.Attack);

            if (isBrawling)
            {
                // Swarm target freely
                bot.holdAttack = false;
                bot.Mover.SetSpeed(bot.stats.moveSpeed);
            }
            else
            {
                // Follow distance rules
                float distanceToLeader = Vector3.Distance(bot.transform.position, transform.position);
                float distanceToEnemy = Vector3.Distance(bot.transform.position, bot.GetTargetEdge());

                float activeBreakDistance = (currentStance == TacticalStance.Offensive) ? activeFormation.breakFormationDistance : bot.stats.attackRange;
                float activeRecallDistance = (currentStance == TacticalStance.Offensive) ? activeFormation.recallDistance : activeFormation.recallDistance * 0.5f;

                if (distanceToLeader > activeRecallDistance)
                {
                    bot.holdAttack = true;
                    loyalBots.Add(bot);
                }
                else if (distanceToEnemy <= activeBreakDistance)
                {
                    bot.holdAttack = false;
                    bot.Mover.SetSpeed(bot.stats.moveSpeed);
                }
                else
                {
                    bot.holdAttack = true;
                    loyalBots.Add(bot);
                }
            }
        }

        loyalBots.AddRange(escorts);

        // Shape the Grid
        if (loyalBots.Count > 0)
        {
            director.ApplyFormation(loyalBots, dangerCenter, activeFormation, transform);
            director.SynchronizeSpeeds(loyalBots, stats.moveSpeed, Mover, tacticalWaypoint, transform, activeFormation.breakFormationDistance);
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
        float targetCommandDistance = activeFormation.safeCommandDistance;

        // Calculate target distance
        if (isEscortActive) targetCommandDistance = activeFormation.safeCommandDistance * 2.5f;
        else if (currentStance == TacticalStance.Offensive) targetCommandDistance = stats.attackRange * 0.8f;
        else if (currentStance == TacticalStance.Defensive) targetCommandDistance *= strategyProfile.defensiveDistanceMultiplier;
        else if (currentStance == TacticalStance.Neutral) targetCommandDistance *= strategyProfile.neutralDistanceMultiplier;

        if (smoothedCommandDistance < 0f) smoothedCommandDistance = targetCommandDistance;
        smoothedCommandDistance = Mathf.Lerp(smoothedCommandDistance, targetCommandDistance, Time.deltaTime * 0.5f);

        Vector3 dirToDanger = (dangerCenter - transform.position);
        dirToDanger.y = 0;
        if (dirToDanger.sqrMagnitude < 0.1f) dirToDanger = transform.forward;

        // Update waypoint and attack hold flag
        if (!isEscortActive && currentStance == TacticalStance.Offensive)
        {
            tacticalWaypoint = dangerCenter - (dirToDanger.normalized * smoothedCommandDistance);
            holdAttack = false;
        }
        else
        {
            tacticalWaypoint = dangerCenter + (-dirToDanger.normalized * smoothedCommandDistance);
            holdAttack = true;
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

        if (closestThreat != null) currentTarget = closestThreat;

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
        if (currentStance == TacticalStance.Offensive) pool = strategyProfile.offensiveFormations;
        else if (currentStance == TacticalStance.Neutral) pool = strategyProfile.neutralFormations;
        else if (currentStance == TacticalStance.Defensive) pool = strategyProfile.defensiveFormations;

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