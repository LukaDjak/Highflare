using DG.Tweening;
using UnityEngine;

public class PickUpController : MonoBehaviour
{
    [Header("References")]
    public Transform itemSocket;
    public float pickUpRange = 3f;
    public LayerMask gunLayer;

    [Header("Drop Settings")]
    public float dropForwardForce = 5f;
    public float dropUpwardForce = 2f;

    private GameObject equippedWeapon;
    private Rigidbody gunRb;
    private Collider gunCol;
    private Animator gunAnim;
    private QuickOutline outline;

    private Tween dockTween;
    private bool isGrappling = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) TryPickUp();
        if (Input.GetKeyDown(KeyCode.Q)) Drop();
    }

    void TryPickUp()
    {
        if (equippedWeapon != null) return;

        Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.SphereCast(ray, 0.5f, out RaycastHit hit, pickUpRange, gunLayer))
        {
            if (hit.collider.CompareTag("Gun"))
                PickUp(hit.collider.gameObject);
        }
    }

    void PickUp(GameObject weapon)
    {
        equippedWeapon = weapon;
        gunRb = weapon.GetComponent<Rigidbody>();
        gunCol = weapon.GetComponent<Collider>();
        gunAnim = weapon.GetComponent<Animator>();
        outline = weapon.GetComponent<QuickOutline>();

        if (weapon.TryGetComponent<Gun>(out var gun))
            gun.enabled = true;

        gunRb.useGravity = false;
        gunCol.enabled = false;
        outline.enabled = false;
        gunAnim.enabled = true;

        weapon.transform.DOMove(itemSocket.position, 0.2f).SetEase(Ease.OutQuad);
        weapon.transform.DORotateQuaternion(itemSocket.rotation, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            weapon.transform.SetParent(itemSocket);
            weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        });

        weapon.layer = LayerMask.NameToLayer("Clipping");

        gunRb.velocity = Vector3.zero;
        gunRb.angularVelocity = Vector3.zero;
    }

    void Drop()
    {
        if (equippedWeapon == null) return;

        equippedWeapon.transform.SetParent(null);
        gunRb.useGravity = true;
        gunCol.enabled = true;
        outline.enabled = true;
        gunAnim.enabled = false;

        equippedWeapon.layer = LayerMask.NameToLayer("Gun");

        gunRb.velocity = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>().velocity;
        gunRb.AddForce(Camera.main.transform.forward * dropForwardForce + Vector3.up * dropUpwardForce, ForceMode.Impulse);

        if (equippedWeapon.TryGetComponent<Gun>(out var gun))
            gun.enabled = false;

        equippedWeapon = null;
        gunRb = null;
        gunCol = null;
        gunAnim = null;
    }

    // Called from Katana swing
    public void DockEquippedWeaponTemporary()
    {
        if (equippedWeapon == null || isGrappling) return;

        dockTween?.Kill();

        Transform gunTransform = equippedWeapon.transform;
        float dockDuration = 5f / 60f;
        float resetDelay = 15f / 60f;

        // Dock
        gunTransform.DOLocalMove(new Vector3(.3f, -.2f, -.4f), dockDuration).SetEase(Ease.OutQuad);
        gunTransform.DOLocalRotate(new Vector3(-10f, 45f, -55f), dockDuration).SetEase(Ease.OutQuad);

        // Reset back to normal after short delay
        dockTween = DOVirtual.DelayedCall(resetDelay, () =>
        {
            gunTransform.DOLocalMove(Vector3.zero, dockDuration).SetEase(Ease.OutQuad);
            gunTransform.DOLocalRotate(Vector3.zero, dockDuration).SetEase(Ease.OutQuad);
        });
    }

    // Called when grapple starts
    public void DockWeaponForGrapple()
    {
        if (equippedWeapon == null) return;

        isGrappling = true;
        dockTween?.Kill();

        Transform gunTransform = equippedWeapon.transform;
        float dockDuration = 0.15f;

        gunTransform.DOLocalMove(new Vector3(-0.75f, -0.13f, -0.08f), dockDuration).SetEase(Ease.OutQuad);
        gunTransform.DOLocalRotate(new Vector3(0f, 0f, 75f), dockDuration).SetEase(Ease.OutQuad);
    }

    // Called when grapple ends
    public void ResetWeaponAfterGrapple()
    {
        if (equippedWeapon == null) return;

        isGrappling = false;
        dockTween?.Kill();

        Transform gunTransform = equippedWeapon.transform;
        float dockDuration = 0.15f;

        gunTransform.DOLocalMove(Vector3.zero, dockDuration).SetEase(Ease.OutQuad);
        gunTransform.DOLocalRotate(Vector3.zero, dockDuration).SetEase(Ease.OutQuad);
    }
}