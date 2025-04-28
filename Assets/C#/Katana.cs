using UnityEngine;

public class Katana : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private ParticleSystem slashEffect;
    [SerializeField] private AudioClip slashClip;
    [SerializeField] private AudioClip hitClip;
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

    //grapple position & rotation
    private Vector3 defaultLocalPosition;
    private Quaternion defaultLocalRotation;
    private readonly Vector3 grappleLocalPosition = new(0.83f, -0.54f, 0.8f);
    private readonly Quaternion grappleLocalRotation = Quaternion.Euler(111f, -6f, 3f);

    private readonly float transitionSpeed = 5f;
    private Collider col;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        col = GetComponent<Collider>();

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
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    float rotateSpeed = 360f; //degrees per second
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
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

        FindObjectOfType<PickUpController>().DockEquippedWeaponTemporary();

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

    //called on animation clip
    public void Shing()
    {
        col.enabled = true;
        RaycastHit[] hits = Physics.SphereCastAll(
            hitOrigin.position,
            hitRadius,
            hitOrigin.forward,
            hitRange,
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length > 0)
        {
            audioSource.pitch = Random.Range(.9f, 1.1f);
            audioSource.PlayOneShot(hitClip);
        }

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.CompareTag("Enemy"))
                hit.transform.GetComponent<Enemy>().DoRagdoll(true);
            if (hit.transform.CompareTag("Barrel"))
                hit.transform.GetComponent<Barrel>().TakeDamage(25);
        }
        col.enabled = false;
    }

    private bool IsCloseToEnemy() => Vector3.Distance(transform.position, grappler.GetGrapplePoint()) < hitRange + 2f;

    private void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitOrigin.position + hitOrigin.forward * hitRange, hitRadius);
    }
}