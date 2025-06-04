using UnityEngine;

public class Katana : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform hitOrigin;
    [SerializeField] private ParticleSystem slashEffect;
    [SerializeField] private AudioClip slashClip, grappleClip, hitClip, enemyHitClip;
    [SerializeField] private Grappler grappler;

    [Header("Properties")]
    [SerializeField] private float swingCooldown = 0.6f;
    [SerializeField] private float hitRange = 2f;
    [SerializeField] private LayerMask hitMask;

    private Animator animator;
    private Collider col;
    private float nextSwingTime;
    private bool swingToRight;
    private bool isUsingKatana;
    private bool hasGrappled = false;

    private Vector3 defaultPos;
    private Quaternion defaultRot;
    private readonly Vector3 grapplePos = new(0.83f, -0.54f, 0.8f);
    private readonly Quaternion grappleRot = Quaternion.Euler(111f, -6f, 3f);

    private const float transitionSpeed = 5f;

    private PickUpController controller;
    private YT_PlayerMovement pm;
    private PlayerControls input;

    private void Awake()
    {
        input = new PlayerControls();
        input.Player.KatanaGrapple.started += _ => {
            isUsingKatana = true;
            grappler.SetGrappleInput(true);
        };
        input.Player.KatanaGrapple.canceled += _ => {
            ReleaseKatana();
            grappler.SetGrappleInput(false);
        };
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Start()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider>();
        controller = FindObjectOfType<PickUpController>();
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<YT_PlayerMovement>();

        defaultPos = transform.localPosition;
        defaultRot = transform.localRotation;
    }

    private void Update()
    {
        if (GameManager.isGameOver)
        {
            if (pm.isGrappling)
                grappler.StopGrapple();
            return;
        }

        UpdateTransform();

        //only proceed if the grapple button is being held
        if (isUsingKatana && Time.time >= nextSwingTime)
        {
            if (grappler.IsGrappling())
            {
                if (IsCloseToEnemy())
                {
                    grappler.StopGrapple();
                    controller.ResetWeaponAfterGrapple();
                    PerformSwing();
                    nextSwingTime = Time.time + swingCooldown;
                }
                return;
            }

            if (!hasGrappled)
            {
                if (grappler.TryGetGrappleTarget(out Vector3 point, out bool isEnemy, out Transform enemyTransform))
                {
                    controller.DockWeaponForGrapple();

                    if (isEnemy)
                    {
                        Rigidbody rb = pm.GetComponent<Rigidbody>();
                        rb.velocity = Vector3.zero;

                        if (pm.IsGrounded())
                            rb.AddForce(Vector3.up * 12f, ForceMode.Impulse);

                        grappler.StartGrapple(point, isEnemy, enemyTransform);
                    }
                    else
                        grappler.StartGrapple(point, isEnemy);

                    hasGrappled = true;
                    return;
                }

                //no valid target — just swing
                PerformSwing();
                nextSwingTime = Time.time + swingCooldown;
                hasGrappled = true;
            }
        }
    }

    private void ReleaseKatana()
    {
        isUsingKatana = false;
        hasGrappled = false;
        if (grappler.IsGrappling())
        {
            controller.ResetWeaponAfterGrapple();
            grappler.StopGrapple();
        }
    }

    private void UpdateTransform()
    {
        bool grappling = grappler.enabled && (grappler.IsGrappling() || grappler.isEnemyGrapple);
        animator.enabled = !grappling;

        Vector3 targetPos = grappling ? grapplePos : defaultPos;
        Quaternion targetRot = grappling ? grappleRot : defaultRot;

        transform.SetLocalPositionAndRotation(Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * transitionSpeed), Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * transitionSpeed));

        if (grappling)
        {
            Vector3 dir = transform.position - grappler.GetGrapplePoint();
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(dir), 360f * Time.deltaTime);
        }
    }

    private void PerformSwing()
    {
        swingToRight = !swingToRight;
        animator.SetBool("SwingToRight", swingToRight);
        animator.SetTrigger("Shing");

        controller.DockEquippedWeaponTemporary();

        PlaySound(slashClip);

        if (slashEffect)
        {
            slashEffect.transform.localRotation = Quaternion.Euler(0, 0, swingToRight ? 0 : 180f);
            slashEffect.Play();
        }
    }

    //called by animation event
    public void Shing()
    {
        col.enabled = true;

        RaycastHit[] hits = Physics.BoxCastAll(
            hitOrigin.position + new Vector3(0, -.2f, 0),
            new Vector3(0.05f, hitRange * 0.5f, 0.05f),
            hitOrigin.forward,
            hitOrigin.rotation * Quaternion.Euler(90f, 0, 0),
            0f,
            hitMask
        );

        if (hits.Length > 0) PlaySound(hitClip, volume: 0.7f);

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.CompareTag("Enemy"))
            {
                PlaySound(enemyHitClip, hit.transform.position);
                hit.transform.GetComponent<Enemy>().DoRagdoll(true);
            }

            if (hit.transform.CompareTag("Barrel"))
                hit.transform.GetComponent<Barrel>().TakeDamage(25);

            if (hit.rigidbody && !hit.transform.CompareTag("Player"))
            {
                float force = 7f * hit.rigidbody.mass;
                hit.rigidbody.AddForce(Vector3.up * force + GameObject.Find("Orientation").transform.forward * force / 2, ForceMode.Impulse);
            }
        }
        col.enabled = false;
    }

    private bool IsCloseToEnemy() => Vector3.Distance(transform.position, grappler.GetGrapplePoint()) < hitRange;

    private void PlaySound(AudioClip clip, Vector3? pos = null, float volume = 1f)
    {
        if (!clip) return;
        SoundManager.instance.PlaySound(clip, pos ?? transform.position, volume, Random.Range(0.9f, 1.1f), 1, transform);
    }

    public void ForceSwingAfterEnemyGrapple()
    {
        controller.ResetWeaponAfterGrapple();
        PerformSwing();
        nextSwingTime = Time.time + swingCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(hitOrigin.position + new Vector3(0, -.2f, 0), hitOrigin.rotation * Quaternion.Euler(90f, 0f, 0f), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.1f, hitRange, 0.1f));
    }
}