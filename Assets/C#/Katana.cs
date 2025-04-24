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

    [Header("Katana Properties")]
    [SerializeField] private float damage = 50f;
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
        if (Input.GetMouseButton(1) && Time.time >= nextSwingTime)
        {
            nextSwingTime = Time.time + swingCooldown;
            SwingKatana();
        }
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

    //call this on animation clips
    public void Shing()
    {
        Debug.Log("Shing shing shing... Shing shing");

        // Sphere cast forward to simulate slash arc
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

            // 🔪 Slice enemy
            if (target.CompareTag("Enemy"))
            {
                Debug.Log("Sliced an enemy: " + target.name);
                target.GetComponent<Enemy>().DoRagdoll(true);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!hitOrigin) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitOrigin.position + hitOrigin.forward * hitRange, hitRadius);
    }

    //add code here for grappling towards enemies
}