using UnityEngine;

public class Crown : MonoBehaviour
{
    [SerializeField] private GameObject particle;
    [SerializeField] private AudioClip pickupSound;

    private void Update() => transform.Rotate(20 * Time.deltaTime, 20 * Time.deltaTime, 20 * Time.deltaTime);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(particle)
                Instantiate(particle, transform.position, Quaternion.identity);
            if (pickupSound)
                SoundManager.instance.PlaySound(pickupSound, transform.position);
            FindObjectOfType<TransitionRoom>().OnCrownCollected(); //show a cutscene while opening that door
            Destroy(gameObject);
        }
    }
}