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

    [Header("Debugging UI")]
    [SerializeField] TMP_Text stateTxt;
    [SerializeField] TMP_Text speedTxt;

    [HideInInspector] public float desiredMoveSpeed, lastDesiredMoveSpeed;
    [HideInInspector] public bool isDashing, isWallRunning, isSliding, isCrouching, keepMomentum;

    private Rigidbody rb;
    private Coroutine lerpSpeedCoroutine;
    [HideInInspector] public float moveSpeed;
    private float lerpDuration;

    private void Start()
    {
        state = MovementState.Walk;
        desiredMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
    }

    private void Update() => StateHandler();

    private void StateHandler()
    {
        if (isDashing)
        {
            state = MovementState.Dash;
            desiredMoveSpeed = dashSpeed;
        }
        else if (isWallRunning)
        {
            state = MovementState.Wallrun;
            desiredMoveSpeed = wallrunSpeed;
        }
        else if (isSliding)
        {
            state = MovementState.Slide;

            if (pm.OnSlope() && rb.velocity.y < 0.1f)
            {
                float currentSpeed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;

                //only increase to slideSpeed if current speed is lower
                if (currentSpeed < slideSpeed)
                {
                    desiredMoveSpeed = slideSpeed;
                    keepMomentum = true;
                    lerpDuration = 5f;
                }
                else
                    //we're already faster, don't reduce it
                    desiredMoveSpeed = currentSpeed;
            }
            else
                desiredMoveSpeed = walkSpeed;
        }
        else if (isCrouching)
        {
            state = MovementState.Crouch;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (!pm.IsGrounded())
        {
            state = MovementState.Air;
            desiredMoveSpeed = moveSpeed;
            keepMomentum = true;
            lerpDuration = .5f;
        }
        else
        {
            if (state == MovementState.Slide)
                keepMomentum = true; // just came from sliding

            state = MovementState.Walk;
            desiredMoveSpeed = walkSpeed;
            lerpDuration = 5f;
        }


        if (desiredMoveSpeed != lastDesiredMoveSpeed && keepMomentum)
        {
            if (lerpSpeedCoroutine != null)
                StopCoroutine(lerpSpeedCoroutine);

            lerpSpeedCoroutine = StartCoroutine(LerpMoveSpeed(lerpDuration));
        }
        else
            moveSpeed = desiredMoveSpeed;

        lastDesiredMoveSpeed = desiredMoveSpeed;
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
            _ => "Unknown, fix ur code dumba**",
        };
    }

    IEnumerator LerpMoveSpeed(float duration)
    {
        float time = 0;
        float startValue = moveSpeed;

        while (time < duration)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        keepMomentum = false;
        lerpDuration = 0;
    }
}

public enum MovementState
{
    Walk,
    Air,
    Crouch,
    Slide,
    Dash,
    Wallrun
}