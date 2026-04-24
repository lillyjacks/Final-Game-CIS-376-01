using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    // Scene references
    public Camera playerCamera;
    public Animator animator;
    public Transform visualRoot;

    // Movement tuning
    public float walkSpeed = 6f;
    public float runSpeed = 10f;
    public float groundAcceleration = 45f;
    public float airAcceleration = 18f;
    public float rotationSmoothSpeed = 12f;

    // Jump and gravity tuning
    public float jumpPower = 7f;
    public float gravity = 20f;
    public float groundedGravity = 5f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    // Look tuning
    public float lookSpeed = 0.12f;
    public float lookXLimit = 45f;
    public float lookSmoothTime = 0.05f;

    // Ground check tuning
    public LayerMask groundMask = Physics.DefaultRaycastLayers;
    public float groundCheckDistance = 0.2f;
    public float groundCheckRadiusScale = 0.9f;
    public float maxGroundAngle = 60f;

    // Crouch is left here for the provided input map, but not enabled yet.
    public float crouchSpeed = 3f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintHeld;
    private bool jumpTriggeredThisFrame;

    private float currentYaw;
    private float targetYaw;
    private float currentPitch;
    private float targetPitch;
    private float yawSmoothVelocity;
    private float pitchSmoothVelocity;
    private float timeSinceGrounded;
    private float timeSinceJumpPressed;
    private Vector3 groundNormal = Vector3.up;
    private Vector3 visualDefaultLocalPosition;

    public bool IsGrounded { get; private set; }
    public bool IsMoving => moveInput.sqrMagnitude > 0.01f;
    public bool IsRunning => sprintHeld && IsMoving;
    public bool IsCrouched => false;
    public float HorizontalSpeed { get; private set; }
    public float VerticalSpeed => rb != null ? rb.linearVelocity.y : 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        controls = new PlayerControls();

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += _ => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += _ => lookInput = Vector2.zero;

        controls.Player.Sprint.performed += _ => sprintHeld = true;
        controls.Player.Sprint.canceled += _ => sprintHeld = false;

        controls.Player.Jump.performed += _ => timeSinceJumpPressed = 0f;

        // Keep the supplied input action but leave crouch disabled for now.
        controls.Player.Crouch.performed += _ => { };
        controls.Player.Crouch.canceled += _ => { };

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        currentYaw = transform.eulerAngles.y;
        targetYaw = currentYaw;
        currentPitch = 0f;
        targetPitch = 0f;

        if (visualRoot != null)
        {
            visualDefaultLocalPosition = visualRoot.localPosition;
        }
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void OnDestroy()
    {
        controls.Dispose();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        timeSinceGrounded += Time.deltaTime;
        timeSinceJumpPressed += Time.deltaTime;

        HandleLook();
        RotateVisuals();
        UpdateAnimatorParameters();
        jumpTriggeredThisFrame = false;
    }

    void LateUpdate()
    {
        if (visualRoot != null)
        {
            // Imported clips can contain baked translation. Keep the mesh attached to the player root.
            visualRoot.localPosition = visualDefaultLocalPosition;
        }
    }

    void FixedUpdate()
    {
        UpdateGroundedState();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    void HandleLook()
    {
        // Mouse input updates target angles first, then we smooth toward them.
        targetPitch -= lookInput.y * lookSpeed;
        targetPitch = Mathf.Clamp(targetPitch, -lookXLimit, lookXLimit);
        targetYaw += lookInput.x * lookSpeed;

        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchSmoothVelocity, lookSmoothTime);
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawSmoothVelocity, lookSmoothTime);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }

        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    void HandleMovement()
    {
        Vector3 inputDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection.Normalize();
        }

        Vector3 moveDirection = IsGrounded
            ? Vector3.ProjectOnPlane(inputDirection, groundNormal).normalized
            : inputDirection;

        float targetSpeed = IsRunning ? runSpeed : walkSpeed;
        Vector3 desiredHorizontalVelocity = moveDirection * targetSpeed;

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 currentHorizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
        float acceleration = IsGrounded ? groundAcceleration : airAcceleration;

        Vector3 nextHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            desiredHorizontalVelocity,
            acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(nextHorizontalVelocity.x, currentVelocity.y, nextHorizontalVelocity.z);
        HorizontalSpeed = nextHorizontalVelocity.magnitude;
    }

    void HandleJump()
    {
        bool canUseBufferedJump = timeSinceJumpPressed <= jumpBufferTime;
        bool canUseCoyoteJump = timeSinceGrounded <= coyoteTime;

        if (!canUseBufferedJump || !canUseCoyoteJump)
        {
            return;
        }

        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpPower;
        rb.linearVelocity = velocity;

        IsGrounded = false;
        timeSinceGrounded = coyoteTime + 1f;
        timeSinceJumpPressed = jumpBufferTime + 1f;
        jumpTriggeredThisFrame = true;
    }

    void ApplyGravity()
    {
        float gravityForce = IsGrounded ? groundedGravity : gravity;
        rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
    }

    void UpdateGroundedState()
    {
        float radius = Mathf.Max(0.05f, capsule.radius * groundCheckRadiusScale);
        Vector3 origin = transform.position + Vector3.up * (radius + 0.05f);

        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, groundCheckDistance + 0.05f, groundMask, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle <= maxGroundAngle)
            {
                IsGrounded = true;
                groundNormal = hit.normal;
                timeSinceGrounded = 0f;
                return;
            }
        }

        IsGrounded = false;
        groundNormal = Vector3.up;
    }

    void RotateVisuals()
    {
        if (visualRoot == null || !IsMoving)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        visualRoot.rotation = Quaternion.Slerp(
            visualRoot.rotation,
            targetRotation,
            rotationSmoothSpeed * Time.deltaTime);
    }

    void UpdateAnimatorParameters()
    {
        if (animator == null)
        {
            return;
        }

        float normalizedSpeed = runSpeed > 0f ? HorizontalSpeed / runSpeed : 0f;

        animator.SetFloat("Speed", normalizedSpeed, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveX", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", moveInput.y, 0.1f, Time.deltaTime);
        animator.SetFloat("VerticalSpeed", VerticalSpeed);
        animator.SetBool("Grounded", IsGrounded);
        animator.SetBool("IsMoving", IsMoving);
        animator.SetBool("IsRunning", IsRunning);
        animator.SetBool("IsCrouching", false);
        animator.SetBool("IsFalling", !IsGrounded && VerticalSpeed < -0.1f);

        if (jumpTriggeredThisFrame)
        {
            animator.SetTrigger("Jump");
        }
    }
}
