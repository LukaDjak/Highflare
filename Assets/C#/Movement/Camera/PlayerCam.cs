using UnityEngine;
using DG.Tweening;
public class PlayerCam : MonoBehaviour
{
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    //Rigidbody rb;
    //ParticleSystem speedPS;

    //private void Start()
    //{    
    //    rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    //    speedPS = GetComponentInChildren<ParticleSystem>(true);
    //}

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX * Time.timeScale;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY * Time.timeScale;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate cam and orientation objects
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }


    public void DoFov(float endValue) => Camera.main.DOFieldOfView(endValue, 0.33f);
    public void DoTilt(float zTilt) => transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.33f);

    //private void LateUpdate() => ChangeParticleAlpha(rb.velocity.magnitude);
    //void ChangeParticleAlpha(float playerSpeed)
    //{
    //    var main = speedPS.main;
    //    if (playerSpeed <= 12) main.startColor = new Color(1f, 1f, 1f, 0f);
    //    else main.startColor = new Color(1f, 1f, 1f, playerSpeed / 120);
    //}
}