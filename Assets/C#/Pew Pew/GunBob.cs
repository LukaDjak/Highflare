using UnityEngine;

public class GunBob : MonoBehaviour
{
    [Header("Sway")]
    public float swayAmount = 0.05f;
    public float maxSway = 0.1f;
    public float swaySmooth = 8f;

    [Header("Sway Rotation")]
    public float rotationAmount = 4f;
    public float maxRotation = 5f;
    public float rotationSmooth = 12f;

    [Header("Bobbing")]
    public float bobSpeed = 8f;
    public float bobAmount = 0.02f;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    private float bobTimer;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        if (transform.childCount == 0) return; 

        Vector2 mouseInput = new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector2 moveInput = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Sway position based on mouse
        Vector3 swayOffset = new Vector3(-mouseInput.x, -mouseInput.y, 0) * swayAmount;
        swayOffset = Vector3.ClampMagnitude(swayOffset, maxSway);

        // Sway rotation based on mouse
        Vector3 swayRot = new Vector3(mouseInput.y, mouseInput.x, -mouseInput.x) * rotationAmount;
        swayRot = Vector3.ClampMagnitude(swayRot, maxRotation);

        targetPosition = initialPosition + swayOffset;
        targetRotation = Quaternion.Euler(swayRot) * initialRotation;

        // Bobbing based on movement
        if (moveInput.magnitude > 0.1f && IsGrounded()) // Optional: Replace with your own grounded check
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobX = Mathf.Sin(bobTimer) * bobAmount;
            float bobY = Mathf.Cos(bobTimer * 2) * bobAmount;

            targetPosition += new Vector3(bobX, bobY, 0);
        }
        else
        {
            bobTimer = 0f;
        }

        // Lerp to smooth motion
        transform.SetLocalPositionAndRotation(
            Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * swaySmooth), 
            Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSmooth));
    }

    bool IsGrounded()
    {
        // Temporary grounded check - replace with your own grounded logic
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
}