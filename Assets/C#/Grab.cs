using UnityEngine;

public class Grab : MonoBehaviour
{
    public LayerMask whatIsGrabbable;

    RaycastHit hit;
    Transform cam;
    GameObject grabbedObj;
    LineRenderer lr;
    SpringJoint joint;

    [Header("Crosshair UI")]
    [SerializeField] private Sprite grabIcon;

    private void Start() => cam = Camera.main.transform;

    private void Update()
    {
        CheckForGrabbable();

        if (Input.GetMouseButtonDown(0))
            GrabObject();

        if (Input.GetMouseButtonUp(0))
            StopGrab();

        if (grabbedObj)
            HoldGrab();
    }

    void CheckForGrabbable()
    {
        if (grabbedObj != null || GameObject.Find("ItemSocket").transform.childCount != 0)
            return;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 10f, whatIsGrabbable))
            CrosshairManager.Instance.SetCrosshair(grabIcon);
        else
            CrosshairManager.Instance.ResetCrosshair();
    }

    void GrabObject()
    {
        if (GameObject.Find("ItemSocket").transform.childCount != 0 || GameManager.isGameOver) return;
        if (Physics.Raycast(cam.position, cam.forward, out hit, 10f, whatIsGrabbable))
        {
            if (hit.transform.GetComponent<Rigidbody>() != null)
            {
                grabbedObj = hit.transform.gameObject;

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

    void StopGrab()
    {
        if (grabbedObj != null)
        {
            Destroy(lr);
            Destroy(joint);
            grabbedObj.GetComponent<Rigidbody>().angularDrag = .05f;
            grabbedObj.GetComponent<Rigidbody>().drag = .0f;
            grabbedObj = null;
        }
    }
}