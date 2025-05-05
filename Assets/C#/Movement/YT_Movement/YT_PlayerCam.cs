using UnityEngine;
using DG.Tweening;
public class YT_PlayerCam : MonoBehaviour
{
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform camHolder;
    [SerializeField] private YT_PlayerMovement pm;
    [SerializeField] private ParticleSystem speedPs;

    float xRot;
    float yRot;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX * GameManager.settings.sensX * Time.timeScale;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY * GameManager.settings.sensY * Time.timeScale;

        yRot += mouseX;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        //rotate cam and orientation objects
        camHolder.rotation = Quaternion.Euler(xRot, yRot, 0);
        orientation.rotation = Quaternion.Euler(0, yRot, 0);
    }


    public void DoFov(float endValue) => Camera.main.DOFieldOfView(endValue, 0.33f);
    public void DoTilt(float zTilt) => transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.33f);

    private void LateUpdate() => ChangeParticleAlpha();
    void ChangeParticleAlpha()
    {
        var main = speedPs.main;
        if (pm.GetMoveSpeed() <= 7) speedPs.Stop();
        else
        {
            if (!speedPs.isPlaying) speedPs.Play(); //optimizejšn
            main.startColor = new Color(1f, 1f, 1f, pm.GetMoveSpeed() / 50);
        }
    }
}