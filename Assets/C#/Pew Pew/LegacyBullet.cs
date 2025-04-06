using UnityEngine;

public class LegacyBullet : MonoBehaviour
{
    [Header("References")]
    public LayerMask enemyLayer;
    private Rigidbody rb;
    public GameObject explosion;
    public GameObject dirtImpact;

    [Header("Stats")]
    public bool useGravity;
    [Range(0, 1f)] public float bounciness;

    [Header("Damage & Lifetime")]
    public float explosionRange;
    public float lifeTime;
    public int maxCollisions;
    public bool explodeOnTouch;

    bool exploded = false;
    int collisions;
    PhysicMaterial pm;

    private void Start()
    {
        Setup();
    }
    void Setup()
    {
        rb = GetComponent<Rigidbody>();

        pm = new PhysicMaterial
        {
            bounciness = bounciness,
            frictionCombine = PhysicMaterialCombine.Minimum,
            bounceCombine = PhysicMaterialCombine.Maximum
        };
        GetComponent<SphereCollider>().material = pm;

        rb.useGravity = useGravity;
    }

    private void Update()
    {
        if (useGravity) rb.AddForce(Vector3.up); //weaker gravity
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Explode();
        if (collisions >= maxCollisions) Explode();
    }

    void Explode() //metoda Explode
    {
        if (explosion != null) //provjerava postoji li objekt pod nazivom explosion
        {
            GameObject particle = Instantiate(explosion, transform.position, Quaternion.identity); //stvaranje objekta na određenoj poziciji i bez rotacije
            if (!exploded)
            {
                exploded = true;
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);
            /*definiramo niz collider-a te u njega stavlja sve objekte određene udaljenosti*/
            foreach (var item in colliders) //petlja koji će za svaki objekt u nizu izvršiti kod koji se nalazi unutar nje
            {
                if (item.TryGetComponent<Rigidbody>(out var rb)) //pokušava dohvatiti Rigidbody komponentu te ju sprema u varijablu ako uspije
                {
                    rb.velocity = Vector3.zero; //resetira brzinu objekta
                    rb.AddExplosionForce(1200f * rb.mass, transform.position, 10f, 10f); //poziva metodu AddExplosionForce
                }
            }
        }
        Invoke(nameof(Destroy2), .05f); //Invoke je metoda preko koje pozivamo metodu nakon određenog delay-aa
    }

    void Destroy2()
    {
        Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Item"))
            collisions++;
    }

    private void OnCollisionEnter(Collision collision)
    {
        collisions++;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
