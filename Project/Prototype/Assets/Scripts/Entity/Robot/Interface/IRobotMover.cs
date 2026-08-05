using UnityEngine;

public interface IRobotMover
{
    void MoveTo(Vector3 destination);
    void StopMovement();
    bool IsMoving();
    void SetSpeed(float speed);
}