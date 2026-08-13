using UnityEngine;

public interface IRobotState
{
    void EnterState(BaseRobotAI robot);
    void UpdateState(BaseRobotAI robot);
    void ExitState(BaseRobotAI robot);
}