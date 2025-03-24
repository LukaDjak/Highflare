using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    Rigidbody rb;
    ParticleSystem speedPS;

    private void Start()
    {    
        rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        speedPS = GetComponentInChildren<ParticleSystem>(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX * Time.timeScale;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY * Time.timeScale;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate cam and orientation objects
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}