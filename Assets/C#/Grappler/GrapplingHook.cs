using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [Header("Grappling Settings")]
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private float maxGrappleDistance = 30f;

    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private Transform player;
    [SerializeField] private Transform lineOrigin;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] GameObject aimAssistObj;

    private SpringJoint springJoint;
    private Vector3 grapplePoint;

    void Start() => lineRenderer.positionCount = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            StartGrapple();
        else if (Input.GetMouseButtonUp(1))
            StopGrapple();

        if (springJoint && lineRenderer.positionCount >= 2)
            DrawRope();

        if (Physics.SphereCast(cam.position, 2f, cam.forward, out RaycastHit hit, maxGrappleDistance, grappleLayer))
        {
            if (!aimAssistObj.activeInHierarchy) aimAssistObj.SetActive(true);
            aimAssistObj.transform.position = Vector3.Lerp(aimAssistObj.transform.position, hit.point, .5f);
        }
        else
            aimAssistObj.SetActive(false);
    }

    void StartGrapple()
    {
        if (Physics.SphereCast(cam.position, 2f, cam.forward, out RaycastHit hit, maxGrappleDistance, grappleLayer))
        {
            grapplePoint = hit.point;
            springJoint = player.gameObject.AddComponent<SpringJoint>();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedAnchor = grapplePoint;

            //adjusting physics properties
            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
            springJoint.maxDistance = distanceFromPoint * 0.7f;
            springJoint.minDistance = distanceFromPoint * 0.2f;
            springJoint.spring = 10f;
            springJoint.damper = 1.5f;
            springJoint.massScale = 4f;

            lineRenderer.positionCount = 2;
        }
    }

    void StopGrapple()
    {
        if (springJoint)
            Destroy(springJoint);
        lineRenderer.positionCount = 0;
    }

    void DrawRope()
    {
        if (lineRenderer.positionCount >= 2)
        {
            lineRenderer.SetPosition(0, lineOrigin.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }
}