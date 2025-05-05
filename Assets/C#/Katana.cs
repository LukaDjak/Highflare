using UnityEngine;
using UnityEngine.Windows;

public class Katana : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private ParticleSystem slashEffect;
    [SerializeField] private AudioClip slashClip;
    [SerializeField] private AudioClip grappleClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip enemyHitClip;
    [SerializeField] private Grappler grappler;

    [Header("Katana Properties")]
    [SerializeField] private float swingCooldown = 0.6f;
    [SerializeField] private float hitRange = 2f;
    [SerializeField] private LayerMask hitMask;

    private Animator animator;
    private float nextSwingTime;
    private bool swingToRight = false;

    //grapple position & rotation
    private Vector3 defaultLocalPosition;
    private Quaternion defaultLocalRotation;
    private readonly Vector3 grappleLocalPosition = new(0.83f, -0.54f, 0.8f);
    private readonly Quaternion grappleLocalRotation = Quaternion.Euler(111f, -6f, 3f);

    private readonly float transitionSpeed = 5f;
    private Collider col;
    private PickUpController controller;
    private bool isUsingKatana;

    private PlayerControls input;
    private void Awake()
    {
        input = new PlayerControls();
        input.Player.KatanaGrapple.started += _ => isUsingKatana = true;
        input.Player.KatanaGrapple.canceled += _ => OnKatanaRelease();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Start()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider>();
        controller = FindObjectOfType<PickUpController>();

        // Cache original transform
        defaultLocalPosition = transform.localPosition;
        defaultLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        if (GameManager.isGameOver) return;
        HandleTransform();

        if (isUsingKatana && Time.time >= nextSwingTime)
        {
            if (grappler.IsGrappling())
            {
                if (IsCloseToEnemy())
                {
                    grappler.StopGrapple();
                    controller.ResetWeaponAfterGrapple();
                    SwingKatana();
                    nextSwingTime = Time.time + swingCooldown;
                }
                return;
            }

            if (grappler.TryGetGrappleTarget(out Vector3 point, out bool isEnemy))
            {
                controller.DockWeaponForGrapple();
                if (isEnemy)
                {
                    GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>().velocity = Vector3.zero;
                    grappler.StartGrapple(point, spring: 100f, damper: 5f, massScale: 1.5f);
                }
                else
                    grappler.StartGrapple(point);

                if (grappleClip)
                    SoundManager.instance.PlaySound(grappleClip, transform.position, 1, Random.Range(.9f, 1.1f));
                return;
            }

            SwingKatana();
            nextSwingTime = Time.time + swingCooldown;
        }
    }

    private void OnKatanaRelease()
    {
        isUsingKatana = false;

        if (grappler.IsGrappling())
        {
            controller.ResetWeaponAfterGrapple();
            grappler.StopGrapple();
        }
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

            //smoothly move to grapple position and rotation
            transform.SetLocalPositionAndRotation(
                Vector3.Lerp(transform.localPosition, grappleLocalPosition, Time.deltaTime * transitionSpeed), 
                Quaternion.Lerp(transform.localRotation, grappleLocalRotation, Time.deltaTime * transitionSpeed));
        }
        else
        {
            if (!animator.enabled) animator.enabled = true;

            //smoothly return to original position and rotation
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
            SoundManager.instance.PlaySound(slashClip, transform.position, 1, Random.Range(.9f, 1.1f), 1, transform);
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
        Vector3 boxHalfExtents = new(0.05f, hitRange * 0.5f,  0.05f);
        Quaternion boxRotation = hitOrigin.rotation * Quaternion.Euler(90f, 0f, 0f);

        RaycastHit[] hits = Physics.BoxCastAll(
            hitOrigin.position + new Vector3(0, -.2f, 0),
            boxHalfExtents,
            hitOrigin.forward,
            boxRotation,
            hitMask
        );

        if (hits.Length > 0)
            SoundManager.instance.PlaySound(hitClip, transform.position, .7f, Random.Range(.9f, 1.1f), 1, transform);


        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.CompareTag("Enemy"))
            {
                SoundManager.instance.PlaySound(enemyHitClip, hit.transform.position);
                hit.transform.GetComponent<Enemy>().DoRagdoll(true);
            }
            if (hit.transform.CompareTag("Barrel"))
                hit.transform.GetComponent<Barrel>().TakeDamage(25);

            Rigidbody rb = hit.rigidbody;
            if (rb != null && !hit.transform.CompareTag("Player"))
            {
                float upwardForce = 7f * rb.mass;
                rb.AddForce(Vector3.up * upwardForce + GameObject.Find("Orientation").transform.forward * upwardForce / 2, ForceMode.Impulse);
            }
        }
        col.enabled = false;
    }

    private bool IsCloseToEnemy() => Vector3.Distance(transform.position, grappler.GetGrapplePoint()) < hitRange;

    private void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        Gizmos.color = Color.red;
        Vector3 boxHalfExtents = new(0.05f, hitRange * 0.5f,  0.05f);
        Gizmos.matrix = Matrix4x4.TRS(hitOrigin.position + new Vector3(0, -.2f, 0), hitOrigin.rotation * Quaternion.Euler(90f, 0f, 0f), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
    }
}