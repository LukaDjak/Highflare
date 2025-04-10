using UnityEngine;

public class WallRun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerCam cam;
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Dash dash;

    [Header("Wallrunning Settings")]
    public float wallRunForce = 20f;
    public float wallClimbSpeed = 3f;
    public float wallGravityForce = 5f;
    public float maxWallRunTime = 2f;
    public float wallJumpForce = 10f;
    public float wallCheckDistance = 0.6f;
    public float minJumpHeight = 1.5f;
    public LayerMask whatIsWall;
    public KeyCode jumpKey = KeyCode.Space;
    public float wallExitCooldown = 0.2f;

    bool wallLeft, wallRight;
    bool isWallRunning;
    bool exitingWall;
    float exitWallTimer;
    float wallRunTimer;

    private float currentWallGravity;


    RaycastHit leftWallHit, rightWallHit;

    void Update()
    {
        CheckForWall();
        StateMachine();
    }

    void FixedUpdate()
    {
        if (isWallRunning)
            WallRunMovement();
    }

    void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);

        Debug.Log($"Wall Left: {wallLeft}, Wall Right: {wallRight}");
    }

    void StateMachine()
    {
        if ((wallLeft || wallRight) && !pm.IsGrounded() && !exitingWall)
        {
            if (!isWallRunning)
                StartWallRun();

            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0)
                StopWallRun();

            if (Input.GetKeyDown(jumpKey))
                WallJump();
        }
        else if (exitingWall)
        {
            if (isWallRunning)
                StopWallRun();

            exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0) exitingWall = false;
        }
        else
        {
            if (isWallRunning)
                StopWallRun();
        }
    }

    void StartWallRun()
    {
        isWallRunning = true;
        wallRunTimer = maxWallRunTime;
        rb.useGravity = false;
        dash.ResetDash();


        // ✅ Reset gravity and vertical velocity
        currentWallGravity = 0f;
        rb.velocity = new Vector3(rb.velocity.x, 2f, rb.velocity.z);

        cam.DoFov(90f);
        cam.DoTilt(wallRight ? 5f : -5f);

        Debug.Log("Started Wall Run");
    }

    void WallRunMovement()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
        if (Vector3.Dot(wallForward, orientation.forward) < 0)
            wallForward = -wallForward;

        rb.velocity = new Vector3(wallForward.x * wallRunForce, rb.velocity.y, wallForward.z * wallRunForce);

        // ✅ Build up custom gravity over time (for smoother sliding)
        currentWallGravity += Time.deltaTime * wallGravityForce;
        rb.AddForce(Vector3.down * currentWallGravity, ForceMode.Force);
    }

    void WallJump()
    {
        // Ensure that we are exiting the wall and applying cooldown
        exitingWall = true;
        exitWallTimer = wallExitCooldown;

        // Get the wall normal based on which side the player is on
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        // Calculate a vector pointing away from the wall and slightly upward
        // The forward direction (relative to the player) + the wall normal
        Vector3 awayFromWall = (transform.forward + wallNormal).normalized;

        // Add upward force to the jump direction to make sure the player jumps away from the wall and not just sideways
        awayFromWall += Vector3.up * 0.5f; // Adjust this to control the upward force

        // Normalize the direction to ensure smooth jump behavior
        awayFromWall.Normalize();

        // Reset the player's velocity (we don't want to carry over any unwanted speed, especially on the Y axis)
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Apply the force in the calculated direction
        rb.AddForce(awayFromWall * wallJumpForce, ForceMode.Impulse);

        // Reset camera effects for smooth walljump transition
        cam.DoTilt(0f);
        cam.DoFov(85f);

        // Stop the wall run immediately after jumping
        StopWallRun();

        Debug.Log("Wall Jump!");
    }



    void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;

        cam.DoFov(85f);
        cam.DoTilt(0f);

        Debug.Log("Stopped Wall Run");
    }
}