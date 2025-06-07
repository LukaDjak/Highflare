using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private bool allowButtonHold = true;
    public KeyCode reloadKey = KeyCode.R;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootForce = 20f;
    [SerializeField] private float spread = 0.1f;
    [SerializeField] private float timeBetweenShots = 0.1f;
    [SerializeField] private int bulletsPerTap = 1;

    [Header("Magazine Settings")]
    public int magazineSize = 12;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Recoil Settings")]
    public float recoilForce = 5f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip reloadSound;

    [HideInInspector] public int bulletsLeft;
    [HideInInspector] public bool isEquipped;

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
        anim = GetComponent<Animator>();
    }

    void Update() => HandleInput();

    void HandleInput()
    {
        if (GameManager.isGameOver || Time.timeScale == 0 || !isEquipped) return;

        if (allowButtonHold)
            shooting = Input.GetMouseButton(0);
        else
            shooting = Input.GetMouseButtonDown(0);

        if (Input.GetKeyDown(reloadKey) && bulletsLeft < magazineSize && !reloading)
            StartReload();

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
            Shoot();
    }

    public void Shoot()
    {
        readyToShoot = false;

        for (int i = 0; i < bulletsPerTap; i++)
        {
            Vector3 direction = GetDirectionWithSpread();
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity, GameObject.Find("Level").transform);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            // Assign shooter
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.shooter = GameObject.FindWithTag("Player");

            rb.AddForce(direction * shootForce, ForceMode.Impulse);

        }

        ApplyRecoil();
        anim.SetTrigger("Shoot");
        muzzleFlash.Play();

        SoundManager.instance.PlaySound(shootSound, firePoint.position, .7f, Random.Range(.9f, 1.1f), 0);

        bulletsLeft--;
        Invoke(nameof(ResetShot), timeBetweenShots);
    }

    //called from enemy script
    public void EnemyShoot(Transform target)
    {
        if (!readyToShoot || reloading || bulletsLeft <= 0) return;

        readyToShoot = false;

        for (int i = 0; i < bulletsPerTap; i++)
        {
            Vector3 direction = GetEnemyDirectionWithSpread(target);
            Debug.DrawRay(firePoint.position, direction * 5f, Color.red, 1f);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation, GameObject.Find("Level").transform);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            // Assign shooter
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.shooter = gameObject;

            rb.AddForce(direction.normalized * shootForce, ForceMode.Impulse);
        }

        if (anim != null) anim.SetTrigger("Shoot");
        if (muzzleFlash != null) muzzleFlash.Play();

        SoundManager.instance.PlaySound(shootSound, firePoint.position, 0.7f, Random.Range(0.9f, 1.1f), 0);

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

    private Vector3 GetEnemyDirectionWithSpread(Transform target)
    {
        Vector3 directionWithoutSpread = target.position - firePoint.position;

        float xSpread = Random.Range(-spread, spread);
        float ySpread = Random.Range(-spread, spread);

        Vector3 spreadDirection = Quaternion.Euler(ySpread, xSpread, 0) * directionWithoutSpread;

        return spreadDirection.normalized;
    }

    void ApplyRecoil()
    {
        if (playerRb != null)
        {
            Vector3 recoilDir = -cam.transform.forward * recoilForce;

            if (recoilForce >= 10) playerRb.velocity = Vector3.zero;
            playerRb.AddForce(recoilDir, ForceMode.Impulse);
        }
    }

    void ResetShot() => readyToShoot = true;

    void StartReload()
    {
        reloading = true;
        CrosshairManager.Instance.SetCrosshair(null);
        CrosshairManager.Instance.StartTimedFill(reloadTime / 2f);
        anim.SetFloat("Duration", 2f / reloadTime);
        anim.SetTrigger("Reload");
        SoundManager.instance.PlaySound(reloadSound, transform.position, .7f, 1.2f, 0);
    }

    //called on animation clip
    public void FinishReload()
    {
        CrosshairManager.Instance.ResetCrosshair();
        bulletsLeft = magazineSize;
        reloading = false;
    }
}