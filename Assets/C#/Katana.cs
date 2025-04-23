using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] private ParticleSystem slashEffect;
    [SerializeField] private AudioClip slashClip;
    
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
        if(Input.GetMouseButtonDown(1))
        {
            swingToRight = !swingToRight;
            animator.SetBool("SwingToRight", swingToRight);
            animator.SetTrigger("Shing");

            if (slashClip) audioSource.PlayOneShot(slashClip);
            if (slashEffect) slashEffect.Play();
        }
    }

    //call this on animation clips
    public void Shing()
    {
        Debug.Log("Shing shing shing... Shing shing");
    }
}