using UnityEngine;

public class Grappler : MonoBehaviour
{
    [Header("Grappling Settings")]
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private float maxGrappleDistance = 30f;

    [Header("Spinning Thingy")]
    [SerializeField] private RectTransform uiGrappleIndicator;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float scaleSpeed = 5f;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;
    [SerializeField] private Transform lineOrigin;
    [SerializeField] private LineRenderer lineRenderer;

    private SpringJoint springJoint;
    private Vector3 grapplePoint;
    private bool isGrappling = false;
    private bool shouldHideIndicator = false;

    void Start() => lineRenderer.positionCount = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            StartGrapple();
        else if (Input.GetMouseButtonUp(1))
            StopGrapple();

        if (springJoint && lineRenderer.positionCount >= 2)
            DrawRope();

        UpdateUIIndicator();
    }

    void StartGrapple()
    {
        if (Physics.SphereCast(cam.transform.position, 2f, cam.transform.forward, out RaycastHit hit, maxGrappleDistance, grappleLayer))
        {
            grapplePoint = hit.point;
            springJoint = player.gameObject.AddComponent<SpringJoint>();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedAnchor = grapplePoint;

            //hiding spinning thing, mark as grappling
            isGrappling = shouldHideIndicator = true;

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
        isGrappling = shouldHideIndicator = false;
    }

    void DrawRope()
    {
        if (lineRenderer.positionCount >= 2)
        {
            lineRenderer.SetPosition(0, lineOrigin.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }

    void UpdateUIIndicator()
    {
        //handle scale-down when grappling
        if (shouldHideIndicator)
        {
            uiGrappleIndicator.localScale = Vector3.Lerp(uiGrappleIndicator.localScale, Vector3.zero, Time.deltaTime * scaleSpeed);
            if (uiGrappleIndicator.localScale.magnitude < 0.01f)
            {
                uiGrappleIndicator.localScale = Vector3.zero;
                uiGrappleIndicator.gameObject.SetActive(false);
                shouldHideIndicator = false; //done hiding
            }
            return;
        }

        if (isGrappling)
        {
            uiGrappleIndicator.gameObject.SetActive(false);
            return;
        }

        if (Physics.SphereCast(cam.transform.position, 2f, cam.transform.forward, out RaycastHit hit, maxGrappleDistance, grappleLayer))
        {
            Vector3 screenPos = cam.WorldToScreenPoint(hit.point);
            uiGrappleIndicator.gameObject.SetActive(true);
            uiGrappleIndicator.position = screenPos;

            uiGrappleIndicator.localEulerAngles += new Vector3(0f, 0f, rotationSpeed * Time.deltaTime);
            uiGrappleIndicator.localScale = Vector3.Lerp(uiGrappleIndicator.localScale, Vector3.one, Time.deltaTime * scaleSpeed);
        }
        else
        {
            uiGrappleIndicator.localScale = Vector3.Lerp(uiGrappleIndicator.localScale, Vector3.zero, Time.deltaTime * scaleSpeed);
            if (uiGrappleIndicator.localScale.magnitude < 0.01f)
                uiGrappleIndicator.gameObject.SetActive(false);
        }
    }
}