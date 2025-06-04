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
    [HideInInspector] public bool isEnemyGrapple = false;

    private void Start() => playerMovement = player.GetComponent<YT_PlayerMovement>();
    private void Update()
    {
        if (isEnemyGrapple)
        {
            Vector3 direction = (grapplePoint - player.position).normalized;
            float distance = Vector3.Distance(player.position, grapplePoint);

            // 👇 Snap player toward enemy
            player.GetComponent<Rigidbody>().velocity = direction * grapplePullSpeed;

            // 👇 Optional smoothing
            // player.position = Vector3.MoveTowards(player.position, grapplePoint, grapplePullSpeed * Time.deltaTime);

            // Stop when close enough
            if (distance < 2f)
            {
                StopGrapple();
                FindObjectOfType<Katana>().ForceSwingAfterEnemyGrapple(); // 👈 call Katana attack
            }
            return;
        }

        UpdateUIIndicator();
    }

    public void StartGrapple(Vector3 targetPoint, bool enemy, float spring = 4.5f, float damper = 7f, float massScale = 4.5f)
    {
        playerMovement.isGrappling = true;
        grapplePoint = targetPoint;
        cam.DoFov(90f);

        if (grappleClip)
            SoundManager.instance.PlaySound(grappleClip, player.position, 1f, Random.Range(0.9f, 1.1f));

        if (enemy)
        {
            isEnemyGrapple = true;
            player.GetComponent<Rigidbody>().velocity = Vector3.zero; // reset movement
            return;
        }

        // default SpringJoint grapple
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

    public void StopGrapple()
    {
        cam.DoFov(85f);
        if (springJoint)
            Destroy(springJoint);

        playerMovement.isGrappling = false;
        isEnemyGrapple = false;
    }


    public bool IsGrappling() => springJoint != null;
    public Vector3 GetGrapplePoint() => grapplePoint;

    public bool TryGetGrappleTarget(out Vector3 targetPoint, out bool isEnemy)
    {
        if (SphereCast(enemyLayer, out targetPoint))
        {
            if (targetPoint != Vector3.zero)
            {
                isEnemy = true;
                return true;
            }
        }
        else if (SphereCast(grappleLayer, out targetPoint))
        {
            isEnemy = false;
            return true;
        }

        targetPoint = Vector3.zero;
        isEnemy = false;
        return false;
    }

    private bool SphereCast(LayerMask layer, out Vector3 point)
    {
        if (Physics.SphereCast(cam.transform.position, 2f, cam.transform.forward, out RaycastHit hit, maxGrappleDistance, layer))
        {
            if (layer == enemyLayer)
            {
                Enemy enemy = hit.transform.GetComponent<Enemy>();
                if (enemy == null || enemy.isDead) { point = Vector3.zero; return false; }
            }

            point = hit.point;
            return true;
        }

        point = Vector3.zero;
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