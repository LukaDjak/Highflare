using UnityEngine;

public class Destructible: MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject fracturedObjectPrefab;
    [SerializeField] private float destructionVelocityThreshold = 15f;

    [Header("Rules")]
    [SerializeField] private bool breakByPlayer = true;
    [SerializeField] private bool breakByGrabbable = true;
    [SerializeField] private bool breakByBullets = true;
    [SerializeField] private bool breakByExplosions = true;

    private bool isDestroyed = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isDestroyed)
            return;

        if (collision.relativeVelocity.magnitude > destructionVelocityThreshold && collision.gameObject.CompareTag("Grabbable") && breakByGrabbable
            || breakByPlayer && collision.gameObject.CompareTag("Player")
            || breakByBullets && collision.gameObject.CompareTag("Bullet"))
            DestroyObject();
    }

    //called from Explosion Source
    public void Explode()
    {
        if (isDestroyed || !breakByExplosions) return;
        DestroyObject();
    }

    private void DestroyObject()
    {
        if (fracturedObjectPrefab != null)
        {
            GameObject fracture = Instantiate(fracturedObjectPrefab, transform.position, transform.rotation, GameObject.Find("Level").transform);
            fracture.transform.localScale = transform.localScale;
        }
        isDestroyed = true;
        Destroy(gameObject);
    }
}