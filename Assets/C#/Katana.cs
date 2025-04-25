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

    // Grapple position & rotation
    private Vector3 defaultLocalPosition;
    private Quaternion defaultLocalRotation;
    private readonly Vector3 grappleLocalPosition = new(0.83f, -0.54f, 0.8f);
    private readonly Quaternion grappleLocalRotation = Quaternion.Euler(111f, -6f, 3f);

    private Vector3 grappleKatanaLocalPos = new(0.83f, -0.54f, 0.8f);
    private Quaternion grappleKatanaLocalRot = Quaternion.Euler(111f, -6f, 3f);
    private readonly float transitionSpeed = 3f; // Adjusted for smooth movement

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Cache original transform
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        HandleTransform();

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

    private bool reachedGrapplePose = false;

    private void LateUpdate()
    {
        if (!grappler.enabled) return;

        if (grappler.IsGrappling())
        {
            if (!animator.enabled) animator.enabled = false;

            //phase 1: transition to fixed grapple pose
            if (!reachedGrapplePose)
            {
                transform.SetLocalPositionAndRotation(
                    Vector3.Lerp(transform.localPosition, grappleLocalPosition, Time.deltaTime * transitionSpeed),
                    Quaternion.Lerp(transform.localRotation, grappleLocalRotation, Time.deltaTime * transitionSpeed)
                );

                if (Vector3.Distance(transform.localPosition, grappleLocalPosition) < 0.01f &&
                    Quaternion.Angle(transform.localRotation, grappleLocalRotation) < 0.5f)
                    reachedGrapplePose = true;
            }
            else
            {
                //phase 2: snap look direction toward target
                Vector3 directionToTarget = transform.position - grappler.GetGrapplePoint();  //inverted direction for -Z axis
                if (directionToTarget.sqrMagnitude > 0.001f)
                {
                    //create a temporary rotation for -Z axis
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * (transitionSpeed * 0.5f));
                }
            }
        }
        else
        {
            if (!animator.enabled) animator.enabled = true;

            //return to default pose
            transform.SetLocalPositionAndRotation(
                Vector3.Lerp(transform.localPosition, defaultLocalPosition, Time.deltaTime * transitionSpeed),
                Quaternion.Lerp(transform.localRotation, defaultLocalRotation, Time.deltaTime * transitionSpeed)
            );
            reachedGrapplePose = false;
        }
    }

    private void HandleTransform()
    {
        if (!grappler.enabled) return;

        if (grappler.IsGrappling())
        {
            if (animator.enabled) animator.enabled = false;

            // Smoothly move to grapple position and rotation
            transform.SetLocalPositionAndRotation(
                Vector3.Lerp(transform.localPosition, grappleLocalPosition, Time.deltaTime * transitionSpeed), 
                Quaternion.Lerp(transform.localRotation, grappleLocalRotation, Time.deltaTime * transitionSpeed));
        }
        else
        {
            if (!animator.enabled) animator.enabled = true;

            // Smoothly return to original position and rotation
            transform.SetLocalPositionAndRotation(
                Vector3.Lerp(transform.localPosition, defaultLocalPosition, Time.deltaTime * transitionSpeed), 
                Quaternion.Lerp(transform.localRotation, defaultLocalRotation, Time.deltaTime * transitionSpeed));
        }
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