using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MovementMechanics : MonoBehaviour
{
    [Header("References")]
    [Tooltip("CharView")]
    public Transform playerCamera;

    public Transform cameraPivot;

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


    [Header("Mouse Look")]

    public float mouseSensitivity = 0.1f;
    public float cameraPos = 1f;
    public float currentDist = 6f;
    public float maxZoom = 10f;
    public float minZoom = 4f;

    private Vector2 lookInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

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

        if (cameraPivot == null)
        {
            Transform foundCamPiv = transform.Find("CameraPivot");
            if (foundCamPiv != null)
            {
                cameraPivot = foundCamPiv;
            }
            else
            {

                Camera childCam = GetComponentInChildren<Camera>();
                if (childCam != null) cameraPivot = childCam.transform;
            }
        }
    }

    private void Update()
    {
        HandleGravity();
        HandleMovement();
        HandleMouseControl();
    }

    // INPUT CALLBACKS
    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
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

        // left shift running if function

        isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

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
        }

        controller.Move(velocity * Time.deltaTime);
    }

   private void HandleMouseControl()
   {
      if (Mouse.current != null)
      {

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            // turns Character movement only left and right.
            float mouseX = mouseDelta.x * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);
            // turns camera with Character without the Character malfunctioning like a bot.
            float mouseY = mouseDelta.y * mouseSensitivity;
            cameraPos -= mouseY;

            // so the Character doesnt break its neck.
            cameraPos = Mathf.Clamp(cameraPos, -65f, 65f);


            // scroll wheel

            Vector2 mouseScroll = Mouse.current.scroll.ReadValue();

            if (mouseScroll.y != 0f)
            {
                currentDist -= mouseScroll.y * 0.2f;

                //limits the chars scroll so it doesnt go insane, hopefully.

                currentDist = Mathf.Clamp(currentDist, minZoom, maxZoom);
            }

            // if camera isnt found add new camera

            if (playerCamera != null)
            {
                playerCamera.localPosition = new Vector3(0f, 0f, -currentDist);
            }


            // so the players camera doesnt act on its own without the Character
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(cameraPos, 0f, 0f);
            }

            // raycast collision systema oh yes

            float targetDist = currentDist;

            RaycastHit hit;

            if (Physics.Raycast(cameraPivot.position, -cameraPivot.forward, out hit, currentDist))
            {
                targetDist = hit.distance - 0.2f;

                targetDist = Mathf.Clamp(targetDist, 0.2f, currentDist);
            }

            if (playerCamera != null)
            {
                playerCamera.localPosition = new Vector3(0f, 0f, -targetDist);
            }
      }
   }           
}
