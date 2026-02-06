using UnityEngine;

public class Grab : MonoBehaviour
{
    public LayerMask whatIsGrabbable;

    RaycastHit hit;
    Transform cam;
    GameObject grabbedObj;
    LineRenderer lr;
    SpringJoint joint;

    [SerializeField] private Transform itemSocket;

    [Header("Crosshair UI")]
    [SerializeField] private Sprite grabIcon;

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 18f;
    [SerializeField] private float throwUpwardForce = 1.5f;

    public static bool IsHoldingObject { get; private set; }
    private static int _consumeAltFireFrame = -1;
    public static bool ConsumeAltFireThisFrame => _consumeAltFireFrame == Time.frameCount;

    private void Start() => cam = Camera.main.transform;

    private void Update()
    {
        CheckForGrabbable();

        if (Input.GetMouseButtonDown(0))
            GrabObject();

        if (grabbedObj && Input.GetMouseButtonDown(1))
        {
            _consumeAltFireFrame = Time.frameCount;
            TryThrow();
        }

        if (Input.GetMouseButtonUp(0))
            StopGrab();

        if (grabbedObj)
            HoldGrab();
    }

    void CheckForGrabbable()
    {
        if (grabbedObj != null || itemSocket.childCount != 0 || PickUpController.IsPickingUpWeapon)
            return;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 10f, whatIsGrabbable) && itemSocket.childCount == 0)
            CrosshairManager.Instance.SetCrosshair(grabIcon);
        else
            CrosshairManager.Instance.ResetCrosshair();
    }

    void GrabObject()
    {
        if (itemSocket.childCount != 0 || GameManager.isGameOver) return;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 10f, whatIsGrabbable))
        {
            if (hit.transform.GetComponent<Rigidbody>() != null)
            {
                grabbedObj = hit.transform.gameObject;

                IsHoldingObject = true;

                joint = grabbedObj.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.minDistance = 0f;
                joint.maxDistance = 0f;
                joint.damper = 2f;
                joint.spring = 15f;
                joint.massScale = 10f * grabbedObj.GetComponent<Rigidbody>().mass;

                grabbedObj.GetComponent<Rigidbody>().angularDrag = 5f;
                grabbedObj.GetComponent<Rigidbody>().drag = 1f;

                lr = grabbedObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = .03f;
                lr.endWidth = .005f;
                lr.startColor = Color.white;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.numCapVertices = 10;
                lr.numCornerVertices = 10;
            }
        }
    }

    void HoldGrab()
    {
        joint.connectedAnchor = cam.position + (cam.forward * 2f);
        lr.SetPosition(0, joint.connectedAnchor);
        lr.SetPosition(1, grabbedObj.transform.position);
    }

    private void TryThrow()
    {
        if (!grabbedObj) return;

        Rigidbody rb = grabbedObj.GetComponent<Rigidbody>();
        StopGrab();

        rb.drag = 0f;
        rb.angularDrag = 0.05f;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 dir = cam.forward;
        Vector3 force = (dir * throwForce + Vector3.up * throwUpwardForce) * rb.mass;
        rb.AddForce(force, ForceMode.Impulse);

        //spin for da feeeeeel
        rb.AddTorque(Random.insideUnitSphere * 0.75f, ForceMode.Impulse);
    }

    void StopGrab()
    {
        if (grabbedObj != null)
        {
            Destroy(lr);
            Destroy(joint);
            grabbedObj.GetComponent<Rigidbody>().angularDrag = .05f;
            grabbedObj.GetComponent<Rigidbody>().drag = .0f;
            grabbedObj = null;

            IsHoldingObject = false;
        }
    }
}