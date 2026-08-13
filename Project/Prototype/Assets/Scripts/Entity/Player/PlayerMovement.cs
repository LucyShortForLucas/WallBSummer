using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float turnSpeed = 10f;

    private Vector2 moveInput;
    private Rigidbody rb;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (mainCamera == null) return;

        if (moveInput.sqrMagnitude >= 0.01f)
        {
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            if (camForward.sqrMagnitude < 0.001f)
            {
                camForward = mainCamera.transform.up;
                camForward.y = 0f;
            }

            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * moveInput.y) + (camRight * moveInput.x);

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                moveDirection.Normalize();

                Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
            }
        }
    }
}