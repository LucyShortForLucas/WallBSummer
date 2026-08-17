using UnityEngine;

public class DespawnState : IRobotState
{
    private float backupTimer = 0f;
    private readonly float maxDespawnTime = 10f;

    public void EnterState(BaseRobotAI robot)
    {
        backupTimer = 0f;

        // Disconnect from the squad so the Leader doesn't wait for them
        if (robot.AssignedLeader != null)
        {
            robot.AssignedLeader.Squad.Remove(robot);
            robot.AssignedLeader = null;
        }

        // Pick a point far away behind them
        Vector3 retreatDirection = -robot.transform.forward;
        Vector3 despawnPoint = robot.transform.position + (retreatDirection * 30f);

        robot.Mover.MoveTo(despawnPoint);
    }

    public void UpdateState(BaseRobotAI robot)
    {
        backupTimer += Time.deltaTime;

        // Check if reached edge, or failsafe timer runs out, delete them
        if (backupTimer >= maxDespawnTime || Vector3.Distance(robot.transform.position, robot.GetTargetEdge()) < 2f)
        {
            GameObject.Destroy(robot.gameObject);
        }
    }

    public void ExitState(BaseRobotAI robot)
    {
    }
}