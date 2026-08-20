using UnityEngine;

public class WanderState : IRobotState
{
    private float changeDirectionTimer = 0f;
    private float totalWanderTime = 0f;

    public void EnterState(BaseRobotAI robot)
    {
        // Wander at halve speed
        robot.Mover.SetSpeed(robot.Stats.MoveSpeed * 0.5f);

        totalWanderTime = Random.Range(8f, 15f);
        SetNewWanderDestination(robot);
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

        totalWanderTime -= Time.deltaTime;
        changeDirectionTimer -= Time.deltaTime;

        // Change direction occasionally
        if (changeDirectionTimer <= 0f || !robot.Mover.IsMoving())
        {
            SetNewWanderDestination(robot);
        }

        // Return to idle 
        if (totalWanderTime <= 0f)
        {
            robot.ChangeState(robot.Idle);
        }
    }

    public void ExitState(BaseRobotAI robot) { }

    private void SetNewWanderDestination(BaseRobotAI robot)
    {
        // Calculate base forward-biased destination
        Vector3 forwardBias = robot.transform.forward * Random.Range(8f, 15f);
        Vector3 lateralDrift = Random.insideUnitSphere * 6f;
        Vector3 newDestination = robot.transform.position + forwardBias + lateralDrift;

        // Separation Steering
        float separationRadius = 5f;
        Collider[] nearbyAllies = Physics.OverlapSphere(robot.transform.position, separationRadius, 1 << robot.gameObject.layer);
        Vector3 separationVector = Vector3.zero;

        foreach (Collider ally in nearbyAllies)
        {
            if (ally.gameObject == robot.gameObject) continue;

            Vector3 awayFromAlly = robot.transform.position - ally.transform.position;
            float distance = awayFromAlly.magnitude;

            // The closer the huzz, the stronger the push away
            if (distance < separationRadius && distance > 0.1f)
            {
                separationVector += awayFromAlly.normalized * (separationRadius - distance);
            }
        }

        // Apply the separation
        newDestination += separationVector * 1.5f;
        newDestination.y = robot.transform.position.y;

        robot.Mover.MoveTo(newDestination);

        // Adjust course
        changeDirectionTimer = Random.Range(3f, 6f);
    }
}