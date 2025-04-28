using UnityEngine;

public class Destructible: MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject fracturedObjectPrefab;
    [SerializeField] private float destructionVelocityThreshold = 15f;
    [SerializeField] private bool canBeDestroyedByPlayer = true;

    private bool isDestroyed = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed)
            return;

        if ((collision.relativeVelocity.magnitude > destructionVelocityThreshold && collision.gameObject.CompareTag("Grabbable"))
            || canBeDestroyedByPlayer && collision.gameObject.CompareTag("Player") 
            || collision.gameObject.CompareTag("Bullet"))
            DestroyObject();
    }

    private void DestroyObject()
    {
        Debug.Log("Fracture");
        if (fracturedObjectPrefab != null)
            Instantiate(fracturedObjectPrefab, transform.position, transform.rotation);
        isDestroyed = true;
        Destroy(gameObject);
    }
}