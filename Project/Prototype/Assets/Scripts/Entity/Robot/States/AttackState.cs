using UnityEngine;

public class AttackState : IRobotState
{
    private float pathUpdateTimer = 0f;
    private readonly float pathUpdateDelay = 0.2f;

    public void EnterState(BaseRobotAI robot)
    {
        pathUpdateTimer = 0f;
    }

    public void UpdateState(BaseRobotAI robot)
    {
        // Return idle if target lost
        if (robot.currentTarget == null)
        {
            robot.ChangeState(robot.Idle);
            return;
        }

        float distanceToTarget = Vector3.Distance(robot.transform.position, robot.GetTargetEdge());

        if (distanceToTarget <= robot.stats.attackRange)
        {
            // Stop and attack if in range
            robot.Mover.StopMovement();

            if (Time.time >= robot.lastAttackTime + robot.stats.attackCooldown)
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
        robot.lastAttackTime = Time.time;

        robot.PerformMeleeAttack(robot.stats.attackDamage);
    }

    private void MoveToTarget(BaseRobotAI robot)
    {
        pathUpdateTimer -= Time.deltaTime;
        if (pathUpdateTimer > 0) return;

        pathUpdateTimer = pathUpdateDelay;

        if (robot.holdAttack && robot.tacticalWaypoint != Vector3.zero)
        {
            robot.Mover.MoveTo(robot.tacticalWaypoint);
        }
        else
        {
            robot.Mover.MoveTo(robot.GetTargetEdge());
        }
    }
}