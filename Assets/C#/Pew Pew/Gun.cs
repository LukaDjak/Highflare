using UnityEngine;
using UnityEngine.Timeline;

public class Gun : MonoBehaviour
{
    [Header("General Settings")]
    public bool allowButtonHold = true;
    public KeyCode reloadKey = KeyCode.R;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootForce = 20f;
    public float spread = 0.1f;
    public float timeBetweenShots = 0.1f;
    public int bulletsPerTap = 1;

    [Header("Magazine Settings")]
    public int magazineSize = 12;
    public float reloadTime = 1.5f;

    [Header("Recoil Settings")]
    public float recoilForce = 5f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;
    private AudioSource audioSource;

    [HideInInspector] public int bulletsLeft;

    private bool shooting;
    private bool readyToShoot;
    private bool reloading;

    private Rigidbody playerRb;
    private Camera cam;
    private Animator anim;

    void Start()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        cam = Camera.main;
        playerRb = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    void Update() => HandleInput();

    void HandleInput()
    {
        if (transform.parent == null || GameManager.isGameOver) return;

        if (allowButtonHold)
            shooting = Input.GetMouseButton(0);
        else
            shooting = Input.GetMouseButtonDown(0);

        if (Input.GetKeyDown(reloadKey) && bulletsLeft < magazineSize && !reloading)
            StartReload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
            Shoot();
    }

    void Shoot()
    {
        readyToShoot = false;

        for (int i = 0; i < bulletsPerTap; i++)
        {
            Vector3 direction = GetDirectionWithSpread();
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(direction * shootForce, ForceMode.Impulse);
        }

        ApplyRecoil();
        anim.SetTrigger("Shoot");
        muzzleFlash.Play();

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(shootSound);

        bulletsLeft--;
        Invoke(nameof(ResetShot), timeBetweenShots);
    }

    Vector3 GetDirectionWithSpread()
    {
        Ray ray = cam.ViewportPointToRay(new(.5f, .5f, 0));

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(75);

        //calculate direction from gun to attack point
        Vector3 directionWithoutSpread = targetPoint - firePoint.position;

        //apply spread on local X and Y axes
        float xSpread = Random.Range(-spread, spread);
        float ySpread = Random.Range(-spread, spread);

        //spread applied in firePoint's local space then converted to world
        Vector3 spreadDirection = Quaternion.Euler(ySpread, xSpread, 0) * directionWithoutSpread;

        return spreadDirection.normalized;
    }

    void ApplyRecoil()
    {
        if (playerRb != null)
        {
            Vector3 recoilDir = -cam.transform.forward * recoilForce;
            playerRb.AddForce(recoilDir, ForceMode.Impulse);
        }
    }

    void ResetShot() => readyToShoot = true;

    void StartReload()
    {
        reloading = true;
        anim.SetFloat("Duration", reloadTime);
        anim.SetTrigger("Reload");
        audioSource.pitch = 1.2f;
        audioSource.PlayOneShot(reloadSound);
        Invoke(nameof(FinishReload), reloadTime);
    }

    void FinishReload()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}