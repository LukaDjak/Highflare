using UnityEngine;

public class OldSliding : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    private OldPlayerMovement pm;
    private Rigidbody rb;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    Vector3 originalScale;
    float xInput, zInput;

    private void Start()
    {
        pm = GetComponent<OldPlayerMovement>();
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

        //NORMAL SLIDE
        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        //SLIDE DOWN A SLOPE
        else
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);

        if (slideTimer <= 0)
            StopSlide();

        Vector3 horizontalVelocity = new(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontalVelocity.magnitude > pm.ms.slideSpeed)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * pm.ms.slideSpeed;
            rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
        }
    }

    private void StopSlide()
    {
        pm.ms.isSliding = false;
        transform.localScale = new Vector3(transform.localScale.x, originalScale.y, transform.localScale.z);
    }
}