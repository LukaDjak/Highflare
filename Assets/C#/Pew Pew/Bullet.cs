using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private GameObject bulletImpact;
    [SerializeField] private GameObject explosion;
    [SerializeField] private AudioClip bulletImpactClip;
    [SerializeField] private AudioClip explosionClip;

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
            Explode();
    }

    void SetupPhysicsMaterial()
    {
        var physMat = new PhysicMaterial
        {
            bounciness = bounciness,
            frictionCombine = PhysicMaterialCombine.Minimum,
            bounceCombine = PhysicMaterialCombine.Maximum
        };

        if (TryGetComponent<Collider>(out var col))
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
                if(col.CompareTag("Enemy"))
                    col.GetComponent<Enemy>().DoRagdoll(true);

                if (col.TryGetComponent<Rigidbody>(out var hitRb))
                    hitRb.AddExplosionForce(explosionForce * hitRb.mass, transform.position, explosionRadius);

                Instantiate(explosion, transform.position, Quaternion.identity);
                SoundManager.instance.PlaySound(explosionClip, explosion.transform.position, .7f);
            }
        }

        Quaternion impactRotation = Quaternion.Euler(Camera.main.transform.eulerAngles + new Vector3(-Camera.main.transform.eulerAngles.x * 2, 180f, 0));
        Instantiate(bulletImpact, transform.position, impactRotation);
        SoundManager.instance.PlaySound(bulletImpactClip, transform.position);
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

    private void OnDrawGizmosSelected()
    {
        if (explosionRadius > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}