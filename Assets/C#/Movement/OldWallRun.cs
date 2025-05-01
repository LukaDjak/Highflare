using UnityEngine;

public class OldWallRun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerCam cam;
    [SerializeField] private OldPlayerMovement pm;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private OldDash dash;

    [Header("Wallrunning Settings")]
    [SerializeField] private float wallRunForce = 20f;
    [SerializeField] private float wallGravityForce = 5f;
    [SerializeField] private float maxWallRunTime = 2f;
    [SerializeField] private float wallJumpForce = 10f;
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private LayerMask whatIsWall;
    public KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private float wallExitCooldown = 0.2f;

    bool wallLeft, wallRight;
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
        if (pm.ms.isWallRunning)
        {
            ApplyWallGravity(); //always apply gravity
            if (Input.GetAxisRaw("Vertical") > 0)
                WallRunMovement(); //only apply movement when moving (logic xD)
        }
    }

    void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    void StateMachine()
    {
        if ((wallLeft || wallRight) && !pm.IsGrounded() && !exitingWall)
        {
            if (!pm.ms.isWallRunning)
                StartWallRun();

            wallRunTimer -= Time.deltaTime;
            if (wallRunTimer <= 0)
            {
                exitingWall = true;
                exitWallTimer = wallExitCooldown;
                StopWallRun();
            }

            if (Input.GetKeyDown(jumpKey))
                WallJump();
        }
        else if (exitingWall)
        {
            if (pm.ms.isWallRunning)
                StopWallRun();

            exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0) exitingWall = false;
        }
        else if (pm.ms.isWallRunning)
            StopWallRun();
    }

    void StartWallRun()
    {
        pm.ms.isWallRunning = true;
        wallRunTimer = maxWallRunTime;
        rb.useGravity = false;
        dash.ResetDash();

        currentWallGravity = 0f;
        rb.velocity = new Vector3(rb.velocity.x, 2f, rb.velocity.z);

        cam.DoFov(90f);
        cam.DoTilt(wallRight ? 7.5f : -7.5f);
    }

    void WallRunMovement()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);
        if (Vector3.Dot(wallForward, orientation.forward) < 0)
            wallForward = -wallForward;

        // Apply movement along the wall
        rb.velocity = new Vector3(wallForward.x * wallRunForce, rb.velocity.y, wallForward.z * wallRunForce);
    }

    private void WallJump()
    {
        //enter exiting wall state
        exitingWall = true;
        exitWallTimer = wallExitCooldown;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = 0.6f * wallJumpForce * transform.up + wallNormal * wallJumpForce;

        //reset y velocity and add force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    void ApplyWallGravity()
    {
        currentWallGravity += Time.fixedDeltaTime * wallGravityForce;
        rb.AddForce(Vector3.down * currentWallGravity, ForceMode.Force);
    }

    void StopWallRun()
    {
        pm.ms.isWallRunning = false;
        rb.useGravity = true;

        cam.DoFov(85f);
        cam.DoTilt(0f);
    }
}