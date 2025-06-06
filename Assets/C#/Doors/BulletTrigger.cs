using UnityEngine;

public class BulletTrigger : MonoBehaviour
{
    public Door door;
    [SerializeField] private bool disableAfterUse = false;
    bool disabled = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && !disabled)
        {
            door.ToggleDoor();
            disabled = disableAfterUse;
        }
    }
}