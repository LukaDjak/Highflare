using UnityEngine;

public class Sliding : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    Vector3 originalScale;
    float xInput, zInput;

    private float lastSlopeSpeed;
    private float slopeMomentumTime;
    [SerializeField] private float momentumKeepTime = 0.5f;

    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
    }

    private void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        zInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(pm.crouchKey) && (xInput != 0 || zInput != 0))
            StartSlide();

        if (Input.GetKeyUp(pm.crouchKey) && pm.ms.isSliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.ms.isSliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        if (pm.ms.isWallRunning || pm.ms.isGrappling) return;

        pm.ms.isSliding = true;
        transform.localScale = new Vector3(transform.localScale.x, originalScale.y * .5f, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;

        if (!pm.OnSlope())
        {
            Vector3 horizontalVelocity = new(rb.velocity.x, 0f, rb.velocity.z);
            if (horizontalVelocity.magnitude > pm.ms.slideSpeed)
            {
                Vector3 clamped = horizontalVelocity.normalized * pm.ms.slideSpeed;
                rb.velocity = new Vector3(clamped.x, rb.velocity.y, clamped.z);
            }
        }
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * zInput + orientation.right * xInput;

        if (pm.OnSlope())
        {
            float slopeAngle = Vector3.Angle(Vector3.up, pm.slopeHit.normal);
            Vector3 moveDir = pm.GetSlopeMoveDirection(inputDirection).normalized;

            //check if going downhill
            bool isGoingDownhill = Vector3.Dot(moveDir, Vector3.down) > 0f;

            rb.AddForce(Vector3.down * 80f, ForceMode.Force); //stick to slope

            if (isGoingDownhill)
            {
                //boost downhill
                float slopeBoost = 1f + (slopeAngle / 45f);
                rb.AddForce(slideForce * slopeBoost * moveDir, ForceMode.Force);

                //save momentum and reset slide timer
                lastSlopeSpeed = rb.velocity.magnitude;
                slopeMomentumTime = Time.time + momentumKeepTime;
                slideTimer = maxSlideTime;
            }
            else
            {
                //sliding up or flat slope: act like flat surface
                rb.AddForce(slideForce * moveDir, ForceMode.Force);
                slideTimer -= Time.fixedDeltaTime;

                if (slideTimer <= 0f)
                    StopSlide();
            }
        }
        else
        {
            Vector3 moveDir = inputDirection.normalized;

            //maintain momentum after leaving slope
            if (Time.time < slopeMomentumTime)
            {
                Vector3 newVel = moveDir * lastSlopeSpeed;
                rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
            }
            else
                rb.AddForce(moveDir * slideForce, ForceMode.Force);

            //decrease slide timer when off slope
            slideTimer -= Time.fixedDeltaTime;
            if (slideTimer <= 0f)
                StopSlide();
        }
    }

    private void StopSlide()
    {
        pm.ms.isSliding = false;
        transform.localScale = new Vector3(transform.localScale.x, originalScale.y, transform.localScale.z);
    }
}