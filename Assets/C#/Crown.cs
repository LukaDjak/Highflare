using UnityEngine;
using UnityEngine.Events;

public class Crown : MonoBehaviour
{
    [SerializeField] private GameObject particle;

    private void Update() => transform.Rotate(20 * Time.deltaTime, 20 * Time.deltaTime, 20 * Time.deltaTime);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(particle)
                Instantiate(particle, transform.position, Quaternion.identity);
            FindObjectOfType<Timer>().enabled = false; //stop the game timer and save it if it's a record
            //FindObjectOfType<TransitionRoom>().OnCrownCollected(); //show a cutscene while opening that door
            Destroy(gameObject);
        }
    }
}