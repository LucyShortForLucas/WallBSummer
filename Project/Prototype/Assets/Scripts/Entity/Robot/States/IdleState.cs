using UnityEngine;

public class IdleState : IRobotState
{
    private float waitTimer;
    private float lookTimer;
    private Quaternion targetRotation;

    public void EnterState(BaseRobotAI robot)
    {
        robot.Mover.StopMovement();
        waitTimer = Random.Range(1f, 3f);
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

        // Smoothly rotate 
        robot.transform.rotation = Quaternion.Slerp(robot.transform.rotation, targetRotation, Time.deltaTime * 2f);

        lookTimer -= Time.deltaTime;
        if (lookTimer <= 0f)
        {
            PickNewLookDirection(robot);
        }

        // Switch to wander when time expires
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0) robot.ChangeState(robot.Wander);
    }

    public void ExitState(BaseRobotAI robot) { }

    private void PickNewLookDirection(BaseRobotAI robot)
    {
        // Pick a random angle
        float randomAngle = Random.Range(-120f, 120f);
        targetRotation = robot.transform.rotation * Quaternion.Euler(0, randomAngle, 0);

        // Hold this look
        lookTimer = Random.Range(1f, 2f);
    }
}