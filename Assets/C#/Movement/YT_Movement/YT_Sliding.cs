using UnityEngine;

public class YT_Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation, playerObj;
    public float maxSlideTime = 1.5f, slideForce = 400f, slideYScale = 0.5f;

    private float startYScale, slideTimer;
    private Vector2 moveInput;

    private Rigidbody rb;
    private YT_PlayerMovement pm;
    private PlayerControls inputActions;

    private void Awake()
    {
        inputActions = new PlayerControls();
        inputActions.Player.Enable();

        //subscribe to movement and slide actions
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => moveInput = Vector2.zero;

        inputActions.Player.Slide.performed += _ => OnSlidePressed();
        inputActions.Player.Slide.canceled += _ => OnSlideReleased();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<YT_PlayerMovement>();
        startYScale = playerObj.localScale.y;
    }

    private void FixedUpdate()
    {
        if (pm.isSliding)
            SlidingMovement();
    }

    private void OnSlidePressed()
    {
        if (moveInput.magnitude > 0.1f && !pm.isGrappling)
            StartSlide();
    }

    private void OnSlideReleased()
    {
        if (pm.isSliding)
            StopSlide();
    }

    private void StartSlide()
    {
        pm.isSliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDir = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDir.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
            rb.AddForce(-pm.slopeHit.normal * 100f, ForceMode.Force); // wall stickiness
        }
        else
            rb.AddForce(pm.GetSlopeMoveDirection(inputDir) * slideForce, ForceMode.Force);
        if (slideTimer <= 0f)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.isSliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}