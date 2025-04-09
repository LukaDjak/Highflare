using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown = 0.2f; //cooldown to prevent excessive bouncing
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 1.2f;
    private float nextJumpTime;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchSpeed;

    [Header("Slide Settings")]
    [SerializeField] private float maxSlopeAngle = 40f;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    private Vector3 originalScale;

    private Rigidbody rb;
    private float desiredMoveSpeed;
    RaycastHit slopeHit;

    [HideInInspector] public bool isDashing;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; //prevent unwanted rotation
        originalScale = transform.localScale;
        desiredMoveSpeed = moveSpeed;
    }

    void Update()
    {
        HandleMovement();
        HandleCrouch();
        HandleJump();
    }

    void HandleMovement()
    {
        if (isDashing) return;

        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (orientation.right * xInput + orientation.forward * zInput).normalized;
        Vector3 targetVelocity = moveDirection * desiredMoveSpeed;

        //maintain Y velocity while setting X/Z velocity
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

        LimitVelocity();
    }

    void HandleJump()
    {
        if (Input.GetKey(jumpKey) && IsGrounded() && Time.time >= nextJumpTime)
        {
            nextJumpTime = Time.time + jumpCooldown; //apply cooldown
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            //isCrouching = true;
            transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);
            desiredMoveSpeed = crouchSpeed;
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            //isCrouching = false;
            transform.localScale = originalScale;
            desiredMoveSpeed = moveSpeed;
        }
    }

    void LimitVelocity()
    {
        Vector3 horizontalVelocity = new(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, originalScale.y * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

    public Vector3 GetSlopeMoveDirection(Vector3 direction) => Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
}