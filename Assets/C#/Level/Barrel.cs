using UnityEngine;

public class Barrel : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject explosionParticles;
    [SerializeField] private AudioClip explosionClip;

    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;

    private float health = 50f;

    private bool hasExploded = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
            Explode();
        else
            TakeDamage(rb.velocity.magnitude);
    }

    private void Explode()
    {
        if(hasExploded) return;

        hasExploded = true;

        if (explosionRadius > 0f)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider col in hitColliders)
            {
                if (col.CompareTag("Enemy"))
                    col.GetComponent<Enemy>().DoRagdoll(true);

                if (col.TryGetComponent<Rigidbody>(out var hitRb))
                    hitRb.AddExplosionForce(explosionForce * hitRb.mass, transform.position, explosionRadius);

                Instantiate(explosionParticles, transform.position, Quaternion.identity);
                SoundManager.instance.PlaySound(explosionClip, transform.position, .7f, Random.Range(0.9f, 1.1f));
            }
        }

        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Explode();
    }
}