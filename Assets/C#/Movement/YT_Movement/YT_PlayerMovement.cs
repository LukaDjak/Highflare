using System.Collections;
using UnityEngine;
using TMPro;

public class YT_PlayerMovement : MonoBehaviour
{
    [SerializeField] private TMP_Text speedTxt, stateTxt;
    [SerializeField] private Transform orientation;

    [Header("Movement Speed")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float slideSpeed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float wallrunSpeed;
    [SerializeField] private float grappleSpeed;
    private float sprintSpeed; //slightly higher speed than walking

    [Header("Multipliers")]
    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;

    [Header("Ground Drag")]
    [SerializeField] private float groundDrag;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool readyToJump = true;

    [Header("Crouching")]
    public float crouchYScale;
    private float startYScale;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask groundLayer;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    public RaycastHit slopeHit;
    private bool exitingSlope;

    private float xInput, zInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    private float moveSpeed, desiredMoveSpeed, lastDesiredMoveSpeed;
    private MoveState state, lastState;
    [HideInInspector] public bool isSliding, isDashing, isWallrunning, isGrappling, isCrouching;
    bool keepMomentum;

    private PlayerControls inputActions;
    private Vector2 moveInput;

    private float movementDisabledTimer = 0f;
    public bool MovementTemporarilyDisabled => movementDisabledTimer > 0f;
    public float GetMoveSpeed() => moveSpeed;

    private void OnEnable()
    {
        inputActions = new PlayerControls();
        inputActions.Enable();
    }

    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        startYScale = transform.localScale.y;
        sprintSpeed = walkSpeed + 3;
    }

    private void Update()
    {
        if (!MovementTemporarilyDisabled)
        {
            HandleInput();
            SpeedControl();
            StateHandler();
        }

        rb.drag = (!MovementTemporarilyDisabled && IsGrounded() && !isGrappling) ? groundDrag : 0f;

        speedTxt.text = $"Speed: {rb.velocity.magnitude:F1}";
        stateTxt.text = state.ToString();

        if (movementDisabledTimer > 0f)
            movementDisabledTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!MovementTemporarilyDisabled)
            MovePlayer();
    }

    private void HandleInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        xInput = GameManager.isGameOver ? 0 : moveInput.x;
        zInput = GameManager.isGameOver ? 0 : moveInput.y;

        //JUMP
        bool jumpHeld = inputActions.Player.Jump.ReadValue<float>() > 0.1f;
        if (jumpHeld && readyToJump && IsGrounded() && !GameManager.isGameOver)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //CROUCH
        bool crouchHeld = inputActions.Player.Crouch.ReadValue<float>() > 0.1f;
        if (crouchHeld && !isCrouching && xInput == 0 && zInput == 0)
        {
            isCrouching = true;
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        else if (!crouchHeld && isCrouching)
        {
            isCrouching = false;
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        if(isGrappling)
        {
            state = MoveState.Grappling;
            desiredMoveSpeed = grappleSpeed;
        }
        else if (isWallrunning)
        {
            state = MoveState.Wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        else if (isDashing)
        {
            state = MoveState.Dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = 200f;
        }
        else if (isSliding)
        {
            state = MoveState.Sliding;
            desiredMoveSpeed = (OnSlope() && rb.velocity.y < .1f) ? slideSpeed : sprintSpeed;
        }
        else if (isCrouching)
        {
            state = MoveState.Crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (IsGrounded())
        {
            state = MoveState.Walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MoveState.Air;
            desiredMoveSpeed = desiredMoveSpeed < sprintSpeed ? walkSpeed : sprintSpeed;
        }

        bool desiredMoveSpeedHasChanged = Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0;

        // DASHING logic: remember if we were dashing
        if (lastState == MoveState.Dashing)
            keepMomentum = true;

        // SLIDING logic
        if (state == MoveState.Sliding)
        {
            if (desiredMoveSpeedHasChanged)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpSlideSpeed());
            }
            else
                moveSpeed = desiredMoveSpeed;
        }
        // DASHING logic
        else if (keepMomentum)
        {
            if (desiredMoveSpeedHasChanged)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpDashSpeed());
            }

            keepMomentum = false; //clear it after dash decay
        }
        else
            moveSpeed = desiredMoveSpeed;

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private IEnumerator SmoothlyLerpDashSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
    }

    private IEnumerator SmoothlyLerpSlideSpeed()
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
    }

    private float speedChangeFactor;

    private void MovePlayer()
    {
        moveDirection = orientation.forward * zInput + orientation.right * xInput;

        //on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(20f * moveSpeed * GetSlopeMoveDirection(moveDirection), ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        //ground and air
        Vector3 force = (IsGrounded() ? 1f : airMultiplier) * 10f * moveSpeed * moveDirection.normalized;
        rb.AddForce(force, ForceMode.Force);

        //turn gravity off while on slope
        if(!isWallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        //limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        //limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;
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

    public Vector3 GetSlopeMoveDirection(Vector3 direction) => Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    public bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

    public void ApplyExternalForce(Vector3 force, float disableDuration = 0.3f)
    {
        rb.velocity = Vector3.zero; // optional: clear current movement
        rb.AddForce(force, ForceMode.Impulse);
        movementDisabledTimer = disableDuration;
    }
}

public enum MoveState { Walking, Crouching, Sliding, Dashing, Wallrunning, Grappling, Air }
public enum SpeedLerpType { Slide, Dash }


//----------WORKS WELL WITH SLIDING------------------
////check if desiredMoveSpeed has changed a lot
//if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
//{
//    StopAllCoroutines();
//    StartCoroutine(SmoothlyLerpMoveSpeed());
//}
//else
//    moveSpeed = desiredMoveSpeed;

//lastDesiredMoveSpeed = desiredMoveSpeed;

//----------WORKS WELL WITH DASH------------------
//bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
//if(lastState == MoveState.Dashing) keepMomentum = true;
//if(desiredMoveSpeedHasChanged)
//{
//    if(keepMomentum)
//    {
//        StopAllCoroutines();
//        StartCoroutine(SmoothlyLerpMoveSpeed());
//    }
//    else
//        moveSpeed = desiredMoveSpeed;
//}
//lastDesiredMoveSpeed = desiredMoveSpeed;
//lastState = state;

//----------COROUTINE THAT WORKS WELL WITH SLIDING------------------
//private IEnumerator SmoothlyLerpMoveSpeed()
//{
//    float time = 0;
//    float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
//    float startValue = moveSpeed;

//    while (time < difference)
//    {
//        moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

//        if (OnSlope())
//        {
//            float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
//            float slopeAngleIncrease = 1 + (slopeAngle / 90f);

//            time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
//        }
//        else
//            time += Time.deltaTime * speedIncreaseMultiplier;
//        yield return null;
//    }
//    moveSpeed = desiredMoveSpeed;
//}

//----------COROUTINE THAT WORKS WELL WITH DASH------------------

//private IEnumerator SmoothlyLerpMoveSpeed()
//{
//    float time = 0;
//    float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
//    float startValue = moveSpeed;

//    float boostFactor = speedChangeFactor;

//    while (time < difference)
//    {
//        moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
//        time += Time.deltaTime * boostFactor;
//        yield return null;
//    }

//    moveSpeed = desiredMoveSpeed;
//    speedChangeFactor = 1f;
//    keepMomentum = false;
//}
