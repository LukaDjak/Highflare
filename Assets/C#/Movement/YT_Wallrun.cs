﻿using UnityEngine;

public class YT_Wallrun : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce = 20f;
    public float wallJumpUpForce = 8f;
    public float wallJumpSideForce = 6f;
    public float maxWallRunTime = 2f;
    [SerializeField] private float wallGravityForce = 7f;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Detection")]
    public float wallCheckDistance = 0.6f;
    public float minJumpHeight = 1f;

    [Header("Exiting")]
    public float exitWallTime = 0.2f;

    [Header("References")]
    public Transform orientation;
    public YT_PlayerCam cam;

    private Rigidbody rb;
    private YT_PlayerMovement pm;

    private RaycastHit leftWallHit, rightWallHit;
    private bool wallLeft, wallRight;

    private bool exitingWall;
    private float wallRunTimer;
    private float exitWallTimer;
    private float currentWallGravity;

    private float horizontalInput;
    private float verticalInput;

    private PlayerControls input;

    private void Awake()
    {
        input = new PlayerControls();
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<YT_PlayerMovement>();
    }

    private void Update()
    {
        Vector2 move = input.Player.Move.ReadValue<Vector2>();
        horizontalInput = move.x;
        verticalInput = move.y;

        CheckForWalls();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.isWallrunning)
        {
            ApplyWallGravity();
            HandleWallMovement();
        }
    }

    private void CheckForWalls()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool IsAboveGround() => !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);

    private void StateMachine()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        float lookDot = Vector3.Dot(orientation.forward.normalized, -wallNormal);
        bool lookingIntoWall = lookDot > 0.5f;

        // Start Wallrun
        if ((wallLeft || wallRight) && verticalInput > 0 && IsAboveGround() && !exitingWall && !lookingIntoWall)
        {
            if (!pm.isWallrunning)
                StartWallRun();

            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (input.Player.Jump.WasPressedThisFrame())
                WallJump();
        }
        // Exit wallrun
        else if (exitingWall)
        {
            if (pm.isWallrunning)
                StopWallRun();

            exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        // Not wallrunning
        else
        {
            if (pm.isWallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        if (exitingWall) return;

        pm.isWallrunning = true;
        wallRunTimer = maxWallRunTime;

        rb.useGravity = false;
        currentWallGravity = 0f;


        rb.velocity = new Vector3(rb.velocity.x, 2f, rb.velocity.z); //upward nudge

        cam.DoFov(90f);
        cam.DoTilt(wallRight ? 7.5f : -7.5f);
    }

    private void HandleWallMovement()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if (Vector3.Dot(orientation.forward, wallForward) < 0)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100f, ForceMode.Force); //stick to wall
    }

    private void StopWallRun()
    {
        pm.isWallrunning = false;
        rb.useGravity = true;

        cam.DoFov(85f);
        cam.DoTilt(0f);
    }

    private void WallJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 jumpForce = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // reset Y
        rb.AddForce(jumpForce, ForceMode.Impulse);
    }

    private void ApplyWallGravity()
    {
        currentWallGravity += Time.fixedDeltaTime * wallGravityForce * 2f;
        rb.AddForce(Vector3.down * currentWallGravity, ForceMode.Force);
    }
}