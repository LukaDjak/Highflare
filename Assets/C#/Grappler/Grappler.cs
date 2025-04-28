﻿using UnityEngine;

public class Grappler : MonoBehaviour
{
    [Header("Grappling Settings")]
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float maxGrappleDistance = 30f;

    [Header("UI Indicator")]
    [SerializeField] private RectTransform uiGrappleIndicator;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float scaleSpeed = 5f;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform player;
    public Transform lineOrigin;

    private SpringJoint springJoint;
    private Vector3 grapplePoint;
    private bool shouldHideIndicator = false;

    private void Update() => UpdateUIIndicator();

    public void StartGrapple(Vector3 targetPoint)
    {
        FindObjectOfType<PickUpController>().DockWeaponForGrapple();
        grapplePoint = targetPoint;
        springJoint = player.gameObject.AddComponent<SpringJoint>();
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.connectedAnchor = grapplePoint;

        float distance = Vector3.Distance(player.position, grapplePoint);
        springJoint.maxDistance = distance * 0.7f;
        springJoint.minDistance = 0.5f;
        springJoint.spring = 25f;
        springJoint.damper = 1f;
        springJoint.massScale = 4f;

        player.GetComponent<PlayerMovement>().ms.isGrappling = true;
    }

    public void StopGrapple()
    {
        FindObjectOfType<PickUpController>().ResetWeaponAfterGrapple();
        if (springJoint)
            Destroy(springJoint);
        player.GetComponent<PlayerMovement>().ms.isGrappling = false;
    }

    public bool IsGrappling() => springJoint != null;
    public Vector3 GetGrapplePoint() => grapplePoint;

    public bool TryGetGrappleTarget(out Vector3 targetPoint)
    {
        //check if can grapple to the enemy first
        if (Physics.SphereCast(cam.transform.position, 2f, cam.transform.forward, out RaycastHit hit, maxGrappleDistance, enemyLayer))
        {
            Enemy enemy = hit.transform.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                targetPoint = hit.point;
                return true;
            }
        }
        //then check if can grapple to the grapple point
        else if (Physics.SphereCast(cam.transform.position, 2f, cam.transform.forward, out hit, maxGrappleDistance, grappleLayer))
        {
            targetPoint = hit.point;
            return true;
        }

        targetPoint = Vector3.zero;
        return false;
    }

    void UpdateUIIndicator()
    {
        if (shouldHideIndicator)
        {
            uiGrappleIndicator.localScale = Vector3.Lerp(uiGrappleIndicator.localScale, Vector3.zero, Time.deltaTime * scaleSpeed);
            if (uiGrappleIndicator.localScale.magnitude < 0.01f)
            {
                uiGrappleIndicator.localScale = Vector3.zero;
                uiGrappleIndicator.gameObject.SetActive(false);
                shouldHideIndicator = false;
            }
            return;
        }

        if (player.GetComponent<PlayerMovement>().ms.isGrappling)
        {
            uiGrappleIndicator.gameObject.SetActive(false);
            return;
        }

        if (Physics.SphereCast(cam.transform.position, 4f, cam.transform.forward, out RaycastHit hit, maxGrappleDistance, grappleLayer))
        {
            Vector3 screenPos = cam.WorldToScreenPoint(hit.transform.position);
            uiGrappleIndicator.gameObject.SetActive(true);
            uiGrappleIndicator.position = screenPos;

            // 🌟 Rotation
            uiGrappleIndicator.localEulerAngles += new Vector3(0f, 0f, rotationSpeed * Time.deltaTime);

            // 🌟 Distance-based scaling
            float distance = Vector3.Distance(cam.transform.position, hit.point);
            float t = 1f - Mathf.Clamp01(distance / maxGrappleDistance); // closer = 1, farther = 0
            float scale = Mathf.Lerp(0.5f, 2.7f, t); // scale range

            Vector3 targetScale = Vector3.one * scale;
            uiGrappleIndicator.localScale = Vector3.Lerp(uiGrappleIndicator.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
        else
        {
            uiGrappleIndicator.localScale = Vector3.Lerp(uiGrappleIndicator.localScale, Vector3.zero, Time.deltaTime * scaleSpeed);
            if (uiGrappleIndicator.localScale.magnitude < 0.01f)
                uiGrappleIndicator.gameObject.SetActive(false);
        }
    }
}