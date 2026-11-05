using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementMechanics : MonoBehaviour
{
    [Header("References")]
    [Tooltip("CharView")]
    public Transform playerCamera;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 8f;
    public float rotateSpeed = 780f;

    [Header("Jumping / Gravity")]
    public float jumpHeight = 1.2f;
    public float gravity = -19.62f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Vector2 moveInput;
    private bool isRunning;
    private bool jumpRequested;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        
        if (playerCamera == null)
        {
            Transform foundCam = transform.Find("CharView");
            if (foundCam != null)
            {
                playerCamera = foundCam;
            }
            else
            {
                
                Camera childCam = GetComponentInChildren<Camera>();
                if (childCam != null) playerCamera = childCam.transform;
            }
        }
    }

    private void Update()
    {
        HandleGravity();
        HandleMovement();
    }

    // INPUT CALLBACKS
    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }

    private void OnRun(InputValue inputValue)
    {
        isRunning = inputValue.isPressed;
    }

    private void OnJump(InputValue inputValue)
    {
        if (isGrounded)
        {
            jumpRequested = true;
        }
    }

    // GRAVITY LOGIC
    private void HandleGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (jumpRequested)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpRequested = false;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputDirection.sqrMagnitude > 1f)
            inputDirection.Normalize();

        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 moveDirection = inputDirection;

            if (playerCamera != null)
            {
                moveDirection = Quaternion.Euler(0f, playerCamera.eulerAngles.y, 0f) * inputDirection;
            }

            controller.Move(moveDirection * targetSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
