using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] private ParticleSystem slashEffect;
    [SerializeField] private AudioClip slashClip;

    [SerializeField] private float swingCooldown = 0.6f;
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

        if (slashClip)
            audioSource.PlayOneShot(slashClip);

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

        //overlap sphere
        //if enemies in that sphere, slice them
        //if destructible, destroy it
        //if collided with anything, leave a shing decal
    }

    //add code here for grappling towards enemies
}