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
        if (pm.ms.isWallRunning) return;

        pm.ms.isSliding = true;
        transform.localScale = new Vector3(transform.localScale.x, originalScale.y * .5f, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;

        //clamp velocity if entering slide with too much speed
        Vector3 horizontalVelocity = new(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVelocity.magnitude > pm.ms.slideSpeed)
        {
            Vector3 clamped = horizontalVelocity.normalized * pm.ms.slideSpeed;
            rb.velocity = new Vector3(clamped.x, rb.velocity.y, clamped.z);
        }
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * zInput + orientation.right * xInput;

        if (pm.IsGrounded() && (!pm.OnSlope() || rb.velocity.y > -0.1f))
        {
            //flat surface slide
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else
        {
            //slope slide
            float slopeAngle = Vector3.Angle(Vector3.up, pm.slopeHit.normal);
            float slopeMultiplier = 2f + (slopeAngle / 45f); //more angle, more force
            Vector3 slopeDir = pm.GetSlopeMoveDirection(inputDirection);

            rb.AddForce(slideForce * slopeMultiplier * slopeDir, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.ms.isSliding = false;
        transform.localScale = new Vector3(transform.localScale.x, originalScale.y, transform.localScale.z);
    }
}