using UnityEngine;

public class GunSway : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float swayAmount = 0.1f; // How much the gun moves
    public float swaySpeed = 4f; // Speed of the sway
    public float swaySpeedIdle = 1f; // Speed when the player is idle
    private Vector3 initialPosition;

    [Header("Camera-Based Sway")]
    public float cameraSwayAmount = 0.02f; // Amount of sway relative to camera movement
    public float cameraSwaySpeed = 2f; // Speed of the camera-based sway

    private Vector3 targetPosition;
    private Vector3 smoothPosition;
    private float swayTime = 0f;

    private bool isMoving = false;
    private Transform playerCamera;

    void Start()
    {
        // Store the initial position of the gun
        initialPosition = transform.localPosition;

        // Get the player's camera
        playerCamera = Camera.main.transform;
    }

    void Update()
    {
        // Determine if the player is moving or idle (you can use your existing movement script for this)
        isMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;

        // Adjust sway speed based on movement or idle
        swayTime += (isMoving ? swaySpeed : swaySpeedIdle) * Time.deltaTime;

        // Apply the sway effect (bobbing up and down)
        targetPosition = initialPosition + new Vector3(0, Mathf.Sin(swayTime) * swayAmount, 0);

        // Apply camera-based sway (horizontal and vertical movement based on camera rotation)
        float cameraPitch = playerCamera.localRotation.eulerAngles.x;  // Up and down camera rotation
        float cameraYaw = playerCamera.localRotation.eulerAngles.y;    // Left and right camera rotation

        // Calculate sway based on camera movement
        float swayX = Mathf.Sin(cameraYaw * Mathf.Deg2Rad) * cameraSwayAmount;
        float swayY = Mathf.Cos(cameraPitch * Mathf.Deg2Rad) * cameraSwayAmount;

        targetPosition += new Vector3(swayX, swayY, 0);

        // Smooth the transition to the new position
        smoothPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * 10f);

        // Apply the smooth position to the gun
        transform.localPosition = smoothPosition;
    }
}
