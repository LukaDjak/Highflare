using UnityEngine;

public class DedObject : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            FindObjectOfType<PauseMenu>().ShowGameOver();
    }
}