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
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashUpForce = 5f;
    [SerializeField] private float dashDuration = 0.25f;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.LeftShift;

    private bool canDash = true;

    private void Update()
    {
        if (Input.GetKeyDown(dashKey) && canDash && !pm.IsGrounded())
            StartCoroutine(PerformDash());
        if (pm.IsGrounded() && !pm.ms.isSliding)
            canDash = true;
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        pm.ms.isDashing = true;

        if (dashBurst)
            dashBurst.Play();

        playerCam.DoFov(90f);
        //CameraShake.Instance.Shake(3f, 0.2f);

        Vector3 dashDirection = orientation.forward * dashForce + orientation.up * dashUpForce;

        //reset velocity and dash
        Vector3 horizontalVel = new(rb.velocity.x, 0f, rb.velocity.z);
        rb.velocity -= horizontalVel; //zero horizontal velocity, preserve vertical
        rb.AddForce(dashDirection, ForceMode.Impulse);

        yield return new WaitForSeconds(dashDuration);

        playerCam.DoFov(85f);
        pm.ms.isDashing = false;
    }

    public void ResetDash() => canDash = true;
}