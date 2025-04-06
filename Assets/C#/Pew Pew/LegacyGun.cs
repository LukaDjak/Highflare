using System.ComponentModel.Design;
using UnityEditor;
using UnityEngine;

public class LegacyGun : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bullet;
    public float shootForce;

    [Header("Gun Settings")]
    public float timeBetweenShooting;
    public float spread; 
    public float reloadTime; 
    public float timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    [Header("Recoil")]
    public float recoilForce;
    Rigidbody playerRB;

    [Header("Input")]
    public KeyCode reloadButton = KeyCode.R;

    [Header("FX")]
    public GameObject muzzleFlash;
    public GUIStyle style;

    [HideInInspector] public int bulletsLeft, bulletsShot;
    bool shooting, readyToShoot, reloading;

    Camera fpsCam;
    public Transform attackPoint;

    //BUG FIXING
    bool allowInvoke = true;

    private void Start() //metoda Start se poziva kada se pokrene igra/scena
    {
        bulletsLeft = magazineSize; //izjednačavamo dvije varijable
        readyToShoot = true; //postavljamo vrijednost varijable na true
        fpsCam = Camera.main; //dodjeljujemo objektu vrijednost tipa main kamere

        playerRB = GameObject.Find("Player").GetComponent<Rigidbody>(); //dohvaćanje Rigidbody komponente sa objekta na sceni pod nazivom "Player"
    }

    private void Update() //metoda Update poziva se svaki frame
    {
        if (transform.parent == GameObject.Find("ItemSocket").transform) //provjerava da li je parent objekta objekt na sceni pod nazivom ItemSocket
            MyInput(); //pozivanje metode MyInput
    }

    void MyInput() //metoda MyInput
    {
        //ALLOW HOLD BUTTON OR NOT
        if (allowButtonHold) shooting = Input.GetMouseButton(0); //varijabla shooting postaje true ako je pritisnut lijevi klik miša
        else shooting = Input.GetMouseButtonDown(0);

        //SHOOT
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0; //postavlja vrijednost varijable na 0
            Shoot(); //poziva metodu Shoot
        }

        if (Input.GetKeyDown(reloadButton) && bulletsLeft < magazineSize && !reloading) Reload(); //poziva se metoda Reload ako se zadovolji uvjet
    }

    public void Shoot()
    {
        readyToShoot = false;

        if (transform.parent == GameObject.Find("ItemSocket"))
        {
            Ray ray = fpsCam.ViewportPointToRay(new(.5f, .5f, 0));

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out RaycastHit hit))
                targetPoint = hit.point;
            else
                targetPoint = ray.GetPoint(75);

            //CALCULATE DIRECTION FROM GUN TO TARGET POINT
            Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

            //ADD SPREAD
            float x = Random.Range(-spread, spread);
            float y = Random.Range(-spread, spread);

            //FINAL DIRECTION
            Vector3 direction = directionWithoutSpread + new Vector3(x, y, 0);

            //INSTANTIATE
            GameObject projectile = Instantiate(bullet, attackPoint.position, Quaternion.identity);
            projectile.transform.forward = direction.normalized;
            projectile.layer = LayerMask.NameToLayer("NoCollision"); //bug fixing

            projectile.GetComponent<Rigidbody>().AddForce(direction.normalized * shootForce, ForceMode.Impulse);

            //ADD RECOIL TO PLAYER
            if (allowInvoke)
            {
                if (recoilForce >= 10)
                    playerRB.velocity = Vector3.zero;
                playerRB.AddForce(-direction.normalized * recoilForce, ForceMode.Impulse);
            }
        }

        //ENEMY SHOOT CODE
        else
        {
            //INSTANTIATE
            GameObject projectile = Instantiate(bullet, attackPoint.position, Quaternion.identity);
            projectile.transform.forward = attackPoint.forward;

            projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * shootForce, ForceMode.Impulse);
        }

        if (muzzleFlash != null)
        {
            GameObject muzzle = Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
            muzzle.transform.forward = attackPoint.forward;
        }

        if(allowInvoke)
        {
            Invoke(nameof(ResetShoot), timeBetweenShooting);
            allowInvoke = false;
        }

        bulletsLeft--;
        bulletsShot++;

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots);
    }

    private void ResetShoot()
    {
        readyToShoot = true;
        allowInvoke = true;
    }

    void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}