using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Cinemachine Setup")]
    [SerializeField] private CinemachineCamera vcam;
    private CinemachineFollow followComponent;

    [Header("Orbit Settings")]
    [SerializeField] private float orbitSensitivityX = 15f;
    [SerializeField] private float orbitSmoothness = 10f;
    [SerializeField] private bool invertX = false;

    [Header("Return Settings")]
    [SerializeField] private bool returnToOriginalRotation = true;
    [SerializeField] private float returnSpeed = 5f;

    private bool isOrbiting;
    private Vector2 dragDelta;

    private float initialYawDeg;
    private float targetYawDeg;
    private float currentYawDeg;

    void Start()
    {
        if (vcam != null)
        {
            followComponent = vcam.GetComponent<CinemachineFollow>();
            if (followComponent != null)
            {
                Vector3 initialOffset = followComponent.FollowOffset;

                targetYawDeg = Mathf.Atan2(initialOffset.x, initialOffset.z) * Mathf.Rad2Deg;
                initialYawDeg = targetYawDeg;
                currentYawDeg = targetYawDeg;
            }
        }
    }

    public void OnOrbitToggle(InputValue value)
    {
        isOrbiting = value.isPressed;
    }

    public void OnOrbitDrag(InputValue value)
    {
        dragDelta = value.Get<Vector2>();
    }

    void Update()
    {
        if (followComponent == null) return;

        CalculateTargetYaw();
        ApplyOrbitOffset();
    }

    private void CalculateTargetYaw()
    {
        if (isOrbiting)
        {
            if (Mathf.Abs(dragDelta.x) > 0.01f)
            {
                float directionMultiplier = invertX ? -1f : 1f;
                targetYawDeg += dragDelta.x * orbitSensitivityX * Time.deltaTime * directionMultiplier;
            }
        }
        else if (returnToOriginalRotation)
        {
            targetYawDeg = Mathf.LerpAngle(targetYawDeg, initialYawDeg, returnSpeed * Time.deltaTime);
        }
    }

    private void ApplyOrbitOffset()
    {
        currentYawDeg = Mathf.LerpAngle(currentYawDeg, targetYawDeg, orbitSmoothness * Time.deltaTime);
        float yawRad = currentYawDeg * Mathf.Deg2Rad;

        Vector3 currentOffset = followComponent.FollowOffset;

        float currentDistance = currentOffset.magnitude;

        if (currentDistance == 0) return;

        float currentPitchRad = Mathf.Asin(currentOffset.y / currentDistance);

        float y = currentDistance * Mathf.Sin(currentPitchRad);
        float xz = currentDistance * Mathf.Cos(currentPitchRad);
        float x = xz * Mathf.Sin(yawRad);
        float z = xz * Mathf.Cos(yawRad);

        followComponent.FollowOffset = new Vector3(x, y, z);
    }
}