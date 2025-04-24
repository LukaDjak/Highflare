using System.Collections;
using UnityEngine;

public class Katana : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private Transform gunSocket;
    [SerializeField] private ParticleSystem slashEffect;
    [SerializeField] private AudioClip slashClip;
    [SerializeField] private LayerMask hitMask; // Enemies, destructibles, etc.
    [SerializeField] private Grappler grappler;

    [Header("Katana Properties")]
    //[SerializeField] private float damage = 50f;
    [SerializeField] private float swingCooldown = 0.6f;
    [SerializeField] private float hitRange = 2f;
    [SerializeField] private float hitRadius = 0.7f;
    private float nextSwingTime = 0f;

    private Animator animator;
    private AudioSource audioSource;

    bool swingToRight = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        //check if player is holding rmb
        if (Input.GetMouseButton(1) && Time.time >= nextSwingTime)
        {
            //if grappling to an enemy, check if we're close enough to trigger a swing
            if (grappler.IsGrappling())
            {
                if (IsCloseToEnemy())
                {
                    //stop the grappler and swing if close to the enemy
                    grappler.StopGrapple();
                    nextSwingTime = Time.time + swingCooldown;
                    SwingKatana();
                }
                return; //prevent swinging the katana if still in the grapple state
            }

            //if no grapple, check if we can grapple to a point
            if (grappler.IsGrappleTargetAvailable())
            {
                if (!grappler.IsGrappling())
                    grappler.StartGrapple(grappler.GetGrapplePoint());

                return; //don't swing the katana if aiming at a grapple point
            }

            //default katana swing if no grapple target is available
            nextSwingTime = Time.time + swingCooldown;
            SwingKatana();
        }

        if (Input.GetMouseButtonUp(1))
            grappler.StopGrapple();
    }

    private void SwingKatana()
    {
        swingToRight = !swingToRight;
        animator.SetBool("SwingToRight", swingToRight);
        animator.SetTrigger("Shing");

        //move the gun a bit so katana doesn't slice it in half
        FindObjectOfType<PickUpController>().DockEquippedWeapon();

        if (slashClip)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(slashClip);
        }

        if (slashEffect)
        {
            slashEffect.transform.localRotation = Quaternion.Euler(0, 0, swingToRight ? 0f : 180f);
            slashEffect.Play();
        }
    }

    //called this on animation clips
    public void Shing()
    {
        Debug.Log("Shing shing shing... Shing shing");

        //sphere cast forward to simulate slash arc
        RaycastHit[] hits = Physics.SphereCastAll(
            hitOrigin.position,
            hitRadius,
            hitOrigin.forward,
            hitRange,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (RaycastHit hit in hits)
        {
            Transform target = hit.transform;

            // 🔪 slice enemy
            if (target.CompareTag("Enemy"))
            {
                Debug.Log("Sliced an enemy: " + target.name);
                target.GetComponent<Enemy>().DoRagdoll(true);
            }
        }
    }

    private bool IsCloseToEnemy() => Vector3.Distance(transform.position, grappler.GetGrapplePoint()) < hitRange + 2;

    private void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitOrigin.position + hitOrigin.forward * hitRange, hitRadius);
    }
}