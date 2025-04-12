using UnityEngine;

public class BulletTrigger : MonoBehaviour
{
    public Door door;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
            door.ToggleDoor();
    }
}