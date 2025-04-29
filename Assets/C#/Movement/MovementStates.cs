using System.Collections;
using TMPro;
using UnityEngine;

public class MovementStates : MonoBehaviour
{
    public MovementState state;
    public PlayerMovement pm;

    [Header("Speed")]
    public float walkSpeed;
    public float crouchSpeed;
    public float slideSpeed;
    public float dashSpeed;
    public float wallrunSpeed;
    public float airSpeed;
    public float grappleSpeed;

    [Header("Multipliers")]
    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;

    [Header("Debugging UI")]
    [SerializeField] TMP_Text stateTxt;
    [SerializeField] TMP_Text speedTxt;

    [HideInInspector] public float desiredMoveSpeed, lastDesiredMoveSpeed, moveSpeed;
    [HideInInspector] public bool isDashing, isWallRunning, isSliding, isCrouching, isGrappling, keepMomentum;
    private Rigidbody rb;

    private void Start()
    {
        state = MovementState.Walk;
        desiredMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
    }

    private void Update() => StateHandler();

    private void StateHandler()
    {
        //GRAPPLING
        if (isGrappling)
        {
            state = MovementState.Grapple;
            desiredMoveSpeed = grappleSpeed;
            keepMomentum = true;
            UpdateUI();
            return; // 👈 prevents any other state from being set
        }

        //DASHING
        else if (isDashing)
        {
            state = MovementState.Dash;
            desiredMoveSpeed = dashSpeed;
        }

        //WALLRUNNING
        else if (isWallRunning)
        {
            state = MovementState.Wallrun;
            desiredMoveSpeed = wallrunSpeed;

        }
        

        // SLIDING
        else if (isSliding)
        {
            state = MovementState.Slide;

            //INCREASE SPEED
            if (pm.OnSlope() && rb.velocity.y < 0.1f)
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
            state = MovementState.Crouch;
            desiredMoveSpeed = crouchSpeed;
        }

        // WALKING
        else if (pm.IsGrounded())
        {
            state = MovementState.Walk;
            desiredMoveSpeed = walkSpeed;
        }

        // AIR
        else
        {
            state = MovementState.Air;

            if (moveSpeed < airSpeed)
                desiredMoveSpeed = airSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(LerpMoveSpeed());
            }
            else
                moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

        //DEACTIVATE KEEP MOMENTUM
        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;

        UpdateUI();
    }

    private void UpdateUI()
    {
        speedTxt.text = "Speed: " + rb.velocity.magnitude.ToString("F1");
        stateTxt.text = state switch
        {
            MovementState.Walk => "Walking",
            MovementState.Air => "Air",
            MovementState.Crouch => "Crouching",
            MovementState.Slide => "Sliding",
            MovementState.Dash => "Dashing",
            MovementState.Wallrun => "Wallrunning",
            MovementState.Grapple => "Grappling",
            _ => "Unknown, fix ur code dumba**",
        };
    }

    IEnumerator LerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (pm.OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, pm.slopeHit.normal);
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
}

public enum MovementState
{
    Walk,
    Air,
    Crouch,
    Slide,
    Dash,
    Wallrun,
    Grapple
}