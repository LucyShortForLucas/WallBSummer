using UnityEngine;

public class WanderState : IRobotState
{
    public void EnterState(BaseRobotAI robot)
    {
        // Calculate random destination
        Vector3 randomDirection = Random.insideUnitSphere * 10f;
        randomDirection += robot.transform.position;
        randomDirection.y = robot.transform.position.y;

        robot.Mover.MoveTo(randomDirection);
    }

    public void UpdateState(BaseRobotAI robot)
    {
        // Check for nearby targets
        Transform bestTarget = robot.GetBestTargetLocally();
        if (bestTarget != null)
        {
            robot.ReportTarget(bestTarget);
            return;
        }

        // Return to idle
        if (!robot.Mover.IsMoving()) robot.ChangeState(robot.Idle);
    }

    public void ExitState(BaseRobotAI robot) { }
}