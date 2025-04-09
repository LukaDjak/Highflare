using UnityEngine;
using System.Collections;

public class Dash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerCam playerCam;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private ParticleSystem dashBurst;

    [Header("Dashing")]
    public float dashForce = 20f;
    public float dashUpForce = 5f;
    public float dashDuration = 0.25f;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.LeftShift;

    private bool canDash = true;

    private void Update()
    {
        if (Input.GetKeyDown(dashKey) && canDash && !pm.IsGrounded())
            StartCoroutine(PerformDash());
        if (pm.IsGrounded())
            canDash = true;
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        pm.isDashing = true;

        if (dashBurst)
            dashBurst.Play();

        playerCam.DoFov(90f);
        //CameraShake.Instance.Shake(3f, 0.2f);

        Vector3 dashDirection = orientation.forward * dashForce + orientation.up * dashUpForce;

        //reset Y velocity and dash
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(dashDirection, ForceMode.Impulse);

        yield return new WaitForSeconds(dashDuration);

        playerCam.DoFov(85f);
        pm.isDashing = false;
    }
}