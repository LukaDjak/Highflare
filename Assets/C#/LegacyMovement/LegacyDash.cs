using UnityEngine;

public class LegacyDash : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public PlayerCam playerCam;
    private Rigidbody rb;
    private LegacyPlayerMovement pm;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpForce;
    public float dashDuration;

    [Header("Cooldown")] //header nam omogućuje dodavanje podnaslova u Inspectoru
    public float dashDelay; //varijabla vidljiva u Inspectoru
    private float dashTimer; //varijabla nije vidljiva u Inspectoru, kao ni drugim klasama

    [Header("Input")]
    public KeyCode dashKey = KeyCode.LeftShift; //varijabla već ima dodijeljenu vrijednost

    bool canDash = true; //varijabla već ima dodijeljenu vrijednost

    private void Start() //metoda Start se poziva kada se pokrene igra/scena
    {
        rb = GetComponent<Rigidbody>(); //pozivanje metode GetComponent, koja dohvaća komponentu Rigidbody i dodjeljuje ju varijabli
        pm = GetComponent<LegacyPlayerMovement>();
    }

    private void Update() //metoda Update poziva se svaki frame
    {
        if (Input.GetKeyDown(dashKey) && pm.state == LegacyPlayerMovement.MovementState.air && canDash) //petlja
            DoDash(); //uvjet se mora zadovoljiti da bi se metoda Dash mogla pozvati

        if (dashTimer > 0)
            dashTimer -= Time.deltaTime;

        if (pm.isGrounded || pm.wallRunning)
            canDash = true;
    }

    private void DoDash()
    {
        if (dashTimer > 0 || pm.isGrounded) return;
        else dashTimer = dashDelay;

        pm.isDashing = true;
        canDash = false;
        Vector3 force = orientation.forward * dashForce + orientation.up * dashUpForce;
        delayedForceToApply = force;
        Invoke(nameof(DelayedDashForce), .025f);
        
        playerCam.DoFov(90f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    Vector3 delayedForceToApply;
    void DelayedDashForce()
    {
        rb.velocity = new(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    void ResetDash()
    {
        playerCam.DoFov(85f);
        pm.isDashing = false;
    }
}