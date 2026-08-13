using UnityEngine;

[RequireComponent(typeof(GroundMover))]

public class GroundRobot : BaseRobotAI
{
    protected override void InitializeRobot()
    {
        Idle = new IdleState();
        Wander = new WanderState();
        Attack = new AttackState();

        ChangeState(Idle);
    }
}