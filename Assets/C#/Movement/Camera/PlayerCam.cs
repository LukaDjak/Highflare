using UnityEngine;
using DG.Tweening;
public class PlayerCam : MonoBehaviour
{
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform camHolder;
    [SerializeField] private OldPlayerMovement pm;
    [SerializeField] private ParticleSystem speedPs;

    float xRotation;
    float yRotation;

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX * GameManager.settings.sensX * Time.timeScale;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY * GameManager.settings.sensY * Time.timeScale;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate cam and orientation objects
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }


    public void DoFov(float endValue) => Camera.main.DOFieldOfView(endValue, 0.33f);
    public void DoTilt(float zTilt) => transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.33f);

    private void LateUpdate() => ChangeParticleAlpha();
    void ChangeParticleAlpha()
    {
        var main = speedPs.main;
        if (pm.ms.moveSpeed <= 12) speedPs.Stop();
        else
        {
            if(!speedPs.isPlaying) speedPs.Play(); //optimizejšn
            main.startColor = new Color(1f, 1f, 1f, pm.ms.moveSpeed / 50);
        }
    }
}