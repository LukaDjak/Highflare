using UnityEngine;

public class YT_Dash : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private YT_PlayerCam playerCam;
    private Rigidbody rb;
    private YT_PlayerMovement pm;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashUpForce;
    [SerializeField] private float dashDuration;

    private PlayerControls input;
    private bool canDash = true;
    private Vector3 force;

    private void Awake() => input = new PlayerControls();

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Dash.performed += ctx => TryDash(); // Subscribe to dash input
    }

    private void OnDisable()
    {
        input.Player.Dash.performed -= ctx => TryDash(); // Unsubscribe
        input.Player.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<YT_PlayerMovement>();
    }

    private void Update()
    {
        if ((pm.IsGrounded() && !pm.isSliding) || pm.isWallrunning)
            canDash = true;
    }

    private void TryDash()
    {
        if (canDash && !pm.IsGrounded())
            PerformDash();
    }

    private void PerformDash()
    {
        pm.isDashing = true;
        canDash = false;

        playerCam.DoFov(90f);
        //CameraShake.Instance.Shake(3f, 0.2f);

        force = orientation.forward * dashForce + orientation.up * dashUpForce;

        Invoke(nameof(ApplyDashForce), 0.0025f);
        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ApplyDashForce() => rb.AddForce(force, ForceMode.Impulse);

    public void ResetDash()
    {
        playerCam.DoFov(85f);
        pm.isDashing = false;
    }
}