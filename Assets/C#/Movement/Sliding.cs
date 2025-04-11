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

        if (Input.GetKeyUp(pm.crouchKey) && pm.isSliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.isSliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        if (pm.isWallRunning) return;

        pm.isSliding = true;
        transform.localScale = new Vector3(transform.localScale.x, originalScale.y * .5f, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }
    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * zInput + orientation.right * xInput;

        //slide on flat surface
        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }

        //slide on slope
        else
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.isSliding = false;
        transform.localScale = new Vector3(transform.localScale.x, originalScale.y, transform.localScale.z);
    }
}