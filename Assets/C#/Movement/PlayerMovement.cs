using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    public MovementStates ms;
    private Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown = 0.2f; //cooldown to prevent excessive bouncing
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 1.2f;
    private float nextJumpTime;

    [Header("Crouch & Slide Settings")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float maxSlopeAngle = 40f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    private Vector3 originalScale;
    public RaycastHit slopeHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; //prevent unwanted rotation
        originalScale = transform.localScale;
    }

    void Update()
    {
        Move();
        Crouch();
        Jump();

        //handle ground drag
        rb.drag = IsGrounded() ? groundDrag : 0;
    }

    private void Move()
    {
        if (ms.isDashing) return;

        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (orientation.right * xInput + orientation.forward * zInput).normalized;
        Vector3 targetVelocity = moveDirection * ms.moveSpeed;

        //maintain Y velocity while setting X/Z velocity
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

        LimitVelocity();
    }

    private void Jump()
    {
        if (Input.GetKey(jumpKey) && IsGrounded() && Time.time >= nextJumpTime)
        {
            nextJumpTime = Time.time + jumpCooldown; //apply cooldown
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void Crouch()
    {
        if (ms.isDashing || ms.isWallRunning || !IsGrounded()) return;
        if (Input.GetKeyDown(crouchKey))
        {
            ms.isCrouching = true;
            transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            ms.isCrouching = false;
            transform.localScale = originalScale;
        }
    }

    private void LimitVelocity()
    {
        if (ms.keepMomentum) return;

        Vector3 horizontalVelocity = new(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVelocity.magnitude > ms.moveSpeed)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * ms.moveSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, originalScale.y * 0.5f + 0.3f))
            return Vector3.Angle(Vector3.up, slopeHit.normal) < maxSlopeAngle && Vector3.Angle(Vector3.up, slopeHit.normal) != 0;
        return false;
    }

    public bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    public Vector3 GetSlopeMoveDirection(Vector3 direction) => Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
}