using UnityEngine;

public class YT_Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation, playerObj;
    public float maxSlideTime = 1.5f, slideForce = 400f, slideYScale = 0.5f;
    public KeyCode slideKey = KeyCode.LeftControl;

    float startYScale, slideTimer, xInput, zInput;
    Rigidbody rb;
    YT_PlayerMovement pm;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<YT_PlayerMovement>();
        startYScale = playerObj.localScale.y;
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        zInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (xInput != 0 || zInput != 0) && !pm.isGrappling)
            StartSlide();

        if (Input.GetKeyUp(slideKey) && pm.isSliding)
            StopSlide();
    }

    void FixedUpdate()
    {
        if (pm.isSliding)
            SlidingMovement();
    }

    void StartSlide()
    {
        pm.isSliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        slideTimer = maxSlideTime;
    }

    void SlidingMovement()
    {
        Vector3 inputDir = orientation.forward * zInput + orientation.right * xInput;

        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDir.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else
            rb.AddForce(pm.GetSlopeMoveDirection(inputDir) * slideForce, ForceMode.Force);

        if (slideTimer <= 0)
            StopSlide();
    }

    void StopSlide()
    {
        pm.isSliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}