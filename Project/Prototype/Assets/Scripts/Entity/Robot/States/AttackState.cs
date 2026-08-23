using UnityEngine;

public class AttackState : IRobotState
{
    private float pathUpdateTimer = 0f;
    private readonly float pathUpdateDelay = 0.2f;

    public void EnterState(BaseRobotAI robot)
    {
        pathUpdateTimer = 0f;

        // Use full speed
        robot.Mover.SetSpeed(robot.Stats.MoveSpeed);
    }

    public void UpdateState(BaseRobotAI robot)
    {
        // Return idle if target lost
        if (robot.CurrentTarget == null)
        {
            robot.ChangeState(robot.Idle);
            return;
        }

        float distanceToTarget = Vector3.Distance(robot.transform.position, robot.GetTargetEdge());

        if (distanceToTarget <= robot.Stats.AttackRange)
        {
            // Stop and attack if in range
            robot.Mover.StopMovement();

            if (Time.time >= robot.LastAttackTime + robot.Stats.AttackCooldown)
            {
                ExecuteAttack(robot);
            }
        }
        else
        {
            // Chase target
            MoveToTarget(robot);
        }
    }

    public void ExitState(BaseRobotAI robot)
    {
        robot.Mover.StopMovement();
    }

    private void ExecuteAttack(BaseRobotAI robot)
    {
        robot.LastAttackTime = Time.time;

        robot.PerformMeleeAttack(robot.Stats.AttackDamage);
    }

    private void MoveToTarget(BaseRobotAI robot)
    {
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer > 0) return;

        pathUpdateTimer = pathUpdateDelay;

        if (robot.HoldAttack && robot.TacticalWaypoint != Vector3.zero)
        {
            robot.Mover.MoveTo(robot.TacticalWaypoint);
        }
        else
        {
            robot.Mover.MoveTo(robot.GetTargetEdge());
        }
    }
}