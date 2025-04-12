using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;

    [Header("Physics Settings")]
    public bool useGravity = false;
    [Range(0f, 1f)] public float bounciness = 0.6f;

    [Header("Lifetime & Collision")]
    public float maxLifetime = 5f;
    public int maxCollisions = 1;
    private float lifetime;
    private int currentCollisions = 0;

    [Header("Explosion Settings")]
    public bool explodeOnImpact = false;
    public float explosionRadius = 0f;
    public float explosionForce = 0f;

    [Header("Target Interaction")]
    public LayerMask enemyLayer;
    public LayerMask destructibleLayer;

    private bool hasExploded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetupPhysicsMaterial();

        lifetime = maxLifetime;
        rb.useGravity = useGravity;
    }

    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f && !hasExploded)
        {
            Explode();
        }
    }

    void SetupPhysicsMaterial()
    {
        var physMat = new PhysicMaterial
        {
            bounciness = bounciness,
            frictionCombine = PhysicMaterialCombine.Minimum,
            bounceCombine = PhysicMaterialCombine.Maximum
        };

        var col = GetComponent<Collider>();
        if (col != null)
            col.material = physMat;
    }

    void OnCollisionEnter(Collision collision)
    {
        currentCollisions++;

        //example: interact with enemy
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            if (explodeOnImpact) Explode();
            else HandleEnemyHit(collision.gameObject);
        }

        // Example: interact with destructible
        if (((1 << collision.gameObject.layer) & destructibleLayer) != 0)
        {
            if (explodeOnImpact) Explode();
            else HandleDestructibleHit(collision.gameObject);
        }

        if (currentCollisions >= maxCollisions && !hasExploded)
        {
            Explode();
            return;
        }
    }

    void Explode()
    {
        hasExploded = true;

        if (explosionRadius > 0f)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider col in hitColliders)
            {
                if (col.TryGetComponent<Rigidbody>(out var hitRb))
                    hitRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                //add explosion effect
            }
        }
        Destroy(gameObject);
    }

    void HandleEnemyHit(GameObject enemy)
    {
        if (enemy.TryGetComponent<Enemy>(out var hitEnemy))
        {
            hitEnemy.DoRagdoll(true);
            Debug.Log("Enemy hit: " + enemy.name);
        }
    }

    void HandleDestructibleHit(GameObject obj)
    {
        // Placeholder — trigger breakable object behavior
        if (obj.TryGetComponent(out Destructible destructible))
        {
            Debug.Log("Destroy" + destructible.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (explosionRadius > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}