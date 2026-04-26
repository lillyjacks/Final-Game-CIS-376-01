using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;

    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float crouchSpeed = 3f;

    public float jumpPower = 7f;
    public float gravity = 10f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    public float defaultHeight = 2f;
    public float crouchHeight = 1f;

    private CharacterController controller;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isCrouching;

    private float rotationX;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new PlayerControls();

        // INPUT BINDINGS
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Sprint.performed += _ => isRunning = true;
        controls.Player.Sprint.canceled += _ => isRunning = false;

        controls.Player.Crouch.performed += _ => isCrouching = true;
        controls.Player.Crouch.canceled += _ => isCrouching = false;

        controls.Player.Jump.performed += _ => Jump();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        float speed = isRunning ? runSpeed : walkSpeed;

        if (isCrouching)
        {
            controller.height = crouchHeight;
            speed = crouchSpeed;
        }
        else
        {
            controller.height = defaultHeight;
        }

        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        controller.Move(move * speed * Time.deltaTime);

        // Gravity
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        rotationX -= lookInput.y * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.Rotate(Vector3.up * lookInput.x * lookSpeed);
    }

    void Jump()
    {
        if (controller.isGrounded)
        {
            velocity.y = jumpPower;
        }
    }
}