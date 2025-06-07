using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && !GameManager.isGameOver)
            FindObjectOfType<PauseMenu>().GameOver();
    }
}