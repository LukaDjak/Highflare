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

    [Header("Keybinds")]
    [SerializeField] private KeyCode forwardKey = KeyCode.W;
    [SerializeField] private KeyCode backwardKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;


    private Vector3 originalScale;
    //private bool isCrouching;

    private Rigidbody rb;
    private float desiredMoveSpeed;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; //prevent unwanted rotation
        originalScale = transform.localScale;
        desiredMoveSpeed = moveSpeed;
    }

    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        HandleMovement();
        HandleCrouch();
        HandleJump();
    }

    void HandleMovement()
    {
        float xInput = 0f;
        float zInput = 0f;

        if (Input.GetKey(forwardKey)) zInput += 1;
        if (Input.GetKey(backwardKey)) zInput -= 1;
        if (Input.GetKey(leftKey)) xInput -= 1;
        if (Input.GetKey(rightKey)) xInput += 1;

        Vector3 moveDirection = (orientation.right * xInput + orientation.forward * zInput).normalized;
        Vector3 targetVelocity = moveDirection * desiredMoveSpeed;

        //maintain Y velocity while setting X/Z velocity
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

        LimitVelocity();
    }

    void HandleJump()
    {
        if (Input.GetKey(jumpKey) && isGrounded && Time.time >= nextJumpTime)
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

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
}