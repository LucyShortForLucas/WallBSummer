using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [Header("Cinemachine Setup")]
    public CinemachineCamera vcam;
    private CinemachineFollow followComponent;

    [Header("Zoom Settings")]
    public float zoomStep = 2f;
    public float minDistance = 2f;
    public float maxDistance = 30f;
    public float zoomSmoothness = 10f;

    // Input & State
    private float scrollInput;
    private float targetDistance;

    void Start()
    {
        if (vcam != null)
        {
            followComponent = vcam.GetComponent<CinemachineFollow>();
            if (followComponent != null)
            {
                targetDistance = followComponent.FollowOffset.magnitude;
            }
        }
    }

    public void OnZoom(InputValue value)
    {
        scrollInput = value.Get<Vector2>().y;
    }

    void Update()
    {
        if (followComponent == null) return;

        CalculateTargetDistance();
        ApplyZoomOffset();
    }

    private void CalculateTargetDistance()
    {
        if (scrollInput != 0)
        {
            float zoomDir = Mathf.Sign(scrollInput);
            targetDistance -= zoomDir * zoomStep;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            scrollInput = 0;
        }
    }

    private void ApplyZoomOffset()
    {
        Vector3 currentOffset = followComponent.FollowOffset;
        float currentDistance = currentOffset.magnitude;

        if (currentDistance == 0) return;

        float smoothedDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothness * Time.deltaTime);

        followComponent.FollowOffset = (currentOffset / currentDistance) * smoothedDistance;
    }
}