using UnityEngine;

public class Katana : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private ParticleSystem slashEffect;
    [SerializeField] private AudioClip slashClip;
    [SerializeField] private Grappler grappler;

    [Header("Katana Properties")]
    [SerializeField] private float swingCooldown = 0.6f;
    [SerializeField] private float hitRange = 2f;
    [SerializeField] private float hitRadius = 0.7f;
    [SerializeField] private LayerMask hitMask;

    private Animator animator;
    private AudioSource audioSource;
    private float nextSwingTime;
    private bool swingToRight = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // RMB Hold (Slash / Grapple)
        if (Input.GetMouseButton(1) && Time.time >= nextSwingTime)
        {
            if (grappler.IsGrappling())
            {
                if (IsCloseToEnemy())
                {
                    grappler.StopGrapple();
                    SwingKatana();
                    nextSwingTime = Time.time + swingCooldown;
                }
                return;
            }

            if (grappler.TryGetGrappleTarget(out Vector3 point))
            {
                grappler.StartGrapple(point);
                return;
            }

            SwingKatana();
            nextSwingTime = Time.time + swingCooldown;
        }

        if (Input.GetMouseButtonUp(1))
            grappler.StopGrapple();
    }

    private void SwingKatana()
    {
        swingToRight = !swingToRight;
        animator.SetBool("SwingToRight", swingToRight);
        animator.SetTrigger("Shing");

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

    public void Shing()
    {
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
            if (hit.transform.CompareTag("Enemy"))
                hit.transform.GetComponent<Enemy>().DoRagdoll(true);
        }
    }

    private bool IsCloseToEnemy() => Vector3.Distance(transform.position, grappler.GetGrapplePoint()) < hitRange + 2f;
    private void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitOrigin.position + hitOrigin.forward * hitRange, hitRadius);
    }
}