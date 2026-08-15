using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GroundMover : MonoBehaviour, IRobotMover
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetSpeed(float speed)
    {
        if (agent != null) agent.speed = speed;
    }

    public void MoveTo(Vector3 destination)
    {
        if (!agent.isOnNavMesh) return;
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    public void StopMovement()
    {
        if (agent.isOnNavMesh) agent.isStopped = true;
    }

    public bool IsMoving()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return false;

        return agent.pathPending || agent.remainingDistance > agent.stoppingDistance;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw path line and destination sphere
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, agent.destination);
            Gizmos.DrawSphere(agent.destination, 0.5f);
        }
    }
#endif
}