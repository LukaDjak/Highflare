using UnityEngine;

public class Grappler : MonoBehaviour
{
    [Header("Grappling Settings")]
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float maxGrappleDistance = 30f;
    [SerializeField] private float grapplePullSpeed = 20f;

    [Header("UI Indicator")]
    [SerializeField] private RectTransform uiGrappleIndicator;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float scaleSpeed = 5f;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("References")]
    [SerializeField] private YT_PlayerCam cam;
    [SerializeField] private Transform player;
    [SerializeField] private AudioClip grappleClip;
    public Transform lineOrigin;

    private SpringJoint springJoint;
    private Vector3 grapplePoint;
    private YT_PlayerMovement playerMovement;
    private bool shouldHideIndicator;
    private bool isGrappleInputHeld = false;
    private Transform grappledEnemy = null;
    [HideInInspector] public bool isEnemyGrapple = false;

    private void Start() => playerMovement = player.GetComponent<YT_PlayerMovement>();
    private void Update()
    {
        if (isEnemyGrapple)
        {
            if (!isGrappleInputHeld)
            {
                StopGrapple();
                return;
            }

            if (grappledEnemy == null)
            {
                StopGrapple();
                return;
            }

            grapplePoint = grappledEnemy.position + Vector3.up * 1.0f; // add Y offset (adjust 1.0f as needed)

            Vector3 direction = (grapplePoint - player.position).normalized;
            float distance = Vector3.Distance(player.position, grapplePoint);

            player.GetComponent<Rigidbody>().velocity = direction * grapplePullSpeed;

            if (distance < 2f)
            {
                StopGrapple();
                FindObjectOfType<Katana>().ForceSwingAfterEnemyGrapple();
            }
            return;
        }

        UpdateUIIndicator();
    }

    public void SetGrappleInput(bool held)
    {
        isGrappleInputHeld = held;

        if (!held && IsGrappling())
        {
            StopGrapple();
        }
    }

    public void StartGrapple(Vector3 targetPoint, bool enemy, Transform enemyTransform = null, float spring = 4.5f, float damper = 7f, float massScale = 4.5f)
    {
        playerMovement.isGrappling = true;
        cam.DoFov(90f);

        if (grappleClip)
            SoundManager.instance.PlaySound(grappleClip, player.position, 1f, Random.Range(0.9f, 1.1f));

        if (enemy)
        {
            isEnemyGrapple = true;
            grappledEnemy = enemyTransform;

            player.GetComponent<Rigidbody>().velocity = Vector3.zero;

            if (grappledEnemy != null)
                grapplePoint = grappledEnemy.position + Vector3.up * 1.0f;
            else
                grapplePoint = targetPoint;

            return;
        }
        else
        {
            // default SpringJoint grapple
            grapplePoint = targetPoint;
            springJoint = player.gameObject.AddComponent<SpringJoint>();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedAnchor = grapplePoint;

            float distance = Vector3.Distance(player.position, grapplePoint);
            springJoint.maxDistance = distance * 0.8f;
            springJoint.minDistance = 0f;
            springJoint.spring = spring;
            springJoint.damper = damper;
            springJoint.massScale = massScale;
        }
    }

    public void StopGrapple()
    {
        cam.DoFov(85f);
        if (springJoint)
            Destroy(springJoint);

        playerMovement.isGrappling = false;
        isEnemyGrapple = false;
        grappledEnemy = null;
    }

    public bool IsGrappling() => springJoint != null;
    public Vector3 GetGrapplePoint() => grapplePoint;

    public bool TryGetGrappleTarget(out Vector3 targetPoint, out bool isEnemy, out Transform targetTransform)
    {
        if (Physics.SphereCast(cam.transform.position, 2f, cam.transform.forward, out RaycastHit hit, maxGrappleDistance, enemyLayer))
        {
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                targetPoint = hit.point;
                isEnemy = true;
                targetTransform = enemy.transform;
                return true;
            }
        }
        else if (Physics.SphereCast(cam.transform.position, 2f, cam.transform.forward, out RaycastHit hitGrapple, maxGrappleDistance, grappleLayer))
        {
            targetPoint = hitGrapple.point;
            isEnemy = false;
            targetTransform = null;
            return true;
        }

        targetPoint = Vector3.zero;
        isEnemy = false;
        targetTransform = null;
        return false;
    }

    private void UpdateUIIndicator()
    {
        if (shouldHideIndicator || playerMovement.isGrappling)
        {
            HideIndicator();
            return;
        }

        if (Physics.SphereCast(cam.transform.position, 4f, cam.transform.forward, out RaycastHit hit, maxGrappleDistance, grappleLayer))
            ShowIndicator(hit);
        else
            HideIndicator();
    }

    private void ShowIndicator(RaycastHit hit)
    {
        uiGrappleIndicator.gameObject.SetActive(true);
        uiGrappleIndicator.position = Camera.main.WorldToScreenPoint(hit.transform.position);
        uiGrappleIndicator.localEulerAngles += new Vector3(0f, 0f, rotationSpeed * Time.deltaTime);

        float t = 1f - Mathf.Clamp01(Vector3.Distance(cam.transform.position, hit.point) / maxGrappleDistance);
        float scale = Mathf.Lerp(0.5f, 2.7f, t);
        Vector3 targetScale = Vector3.one * scale;

        uiGrappleIndicator.localScale = Vector3.Lerp(uiGrappleIndicator.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    private void HideIndicator()
    {
        uiGrappleIndicator.localScale = Vector3.Lerp(uiGrappleIndicator.localScale, Vector3.zero, Time.deltaTime * scaleSpeed);

        if (uiGrappleIndicator.localScale.magnitude < 0.01f)
        {
            uiGrappleIndicator.localScale = Vector3.zero;
            uiGrappleIndicator.gameObject.SetActive(false);
            shouldHideIndicator = false;
        }
    }
}