using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;

    [Header("Movement Settings")]
    public float moveSpeed = 7f;

    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode forwardKey = KeyCode.W;
    [SerializeField] private KeyCode backwardKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; //prevent unwanted rotation
    }

    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundLayer);

        HandleMovement();

        if (Input.GetKeyDown(jumpKey) && isGrounded)
            Jump();
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
        Vector3 targetVelocity = moveDirection * moveSpeed;

        //maintain Y velocity while setting X/Z velocity
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

        LimitVelocity();
    }

    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void LimitVelocity()
    {
        //this makes sure the velocity doesn't go over maximum move speed
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVelocity.magnitude > moveSpeed)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }
}