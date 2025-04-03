using DG.Tweening;
using System.Collections;
using UnityEngine;

public class LegacyPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float airSpeed;
    public float dashSpeed;

    [Space(5)]
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    [Space(5)]
    public float groundDrag;

    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    [HideInInspector] public bool isGrounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("References")]
    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;

    [HideInInspector] public MovementState state;
    private MovementState lastState;
    public enum MovementState
    {
        walking,
        wallrunning,
        crouching,
        sliding,
        dashing,
        air
    }

    [HideInInspector] public bool isSliding;
    [HideInInspector] public bool isCrouching;
    [HideInInspector] public bool wallRunning;
    [HideInInspector] public bool isClimbing;
    [HideInInspector] public bool isDashing;
    bool keepMomentum;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        //GROUND CHECK
        if (!wallRunning)
        isGrounded = Physics.SphereCast(transform.position, .3f, Vector3.down, out _, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        //GROUND DRAG
        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate() => MovePlayer();

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //JUMP
        if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //START CROUCH
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0 && !isSliding)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            isCrouching = true;
        }

        //STOP CROUCH
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

            isCrouching = false;
        }
    }

    private void StateHandler()
    {
        //DASHING
        if(isDashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
        }

        //WALLRUNNING
        else if (wallRunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
           
        }

        // SLIDING
        else if (isSliding)
        {
            state = MovementState.sliding;

            //INCREASE SPEED
            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
            }

            else
                desiredMoveSpeed = walkSpeed;
        }

        // CROUCHING
        else if (isCrouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // WALKING
        else if (isGrounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // AIR
        else
        {
            state = MovementState.air;

            if (moveSpeed < airSpeed)
                desiredMoveSpeed = airSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
                moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;

        //DEACTIVATE KEEP MOMENTUM
        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        keepMomentum = false;
    }

    private void MovePlayer()
    {
        //if (climbingScript.exitingWall) return;
        if (state == MovementState.dashing) return;    

        // CALCULATE MOVEMENT DIRECTION
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // ON SLOPE
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(20f * moveSpeed * GetSlopeMoveDirection(moveDirection), ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        //ON GROUND
        else if (isGrounded)
            rb.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);

        //IN AIR
        else if (!isGrounded)
            rb.AddForce(10f * airMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);

        //TURN OFF GRAVITY WHEN ON SLOPE
        if(!wallRunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        //LIMIT VELOCITY ON SLOPE
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        //LIMIT VELOCITY ON GROUND & AIR
        else
        {
            Vector3 flatVel = new(rb.velocity.x, 0f, rb.velocity.z);

            //LIMIT VELOCITY IF NEEDED
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;

        //RESET VELOCITY & ADD FORCE
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
