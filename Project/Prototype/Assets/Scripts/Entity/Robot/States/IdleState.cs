using UnityEngine;

public class IdleState : IRobotState
{
    private float waitTimer;

    public void EnterState(BaseRobotAI robot)
    {
        robot.Mover.StopMovement();
        waitTimer = Random.Range(0f, 3f);
    }

    public void UpdateState(BaseRobotAI robot)
    {
        // Scan for enemies
        Transform bestTarget = robot.GetBestTargetLocally();
        if (bestTarget != null)
        {
            robot.ReportTarget(bestTarget);
            return;
        }

        // Switch to wander when time expires
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0) robot.ChangeState(robot.Wander);
    }

    public void ExitState(BaseRobotAI robot) { }
}