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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) TryPickUp();
        if (Input.GetKeyDown(KeyCode.Q)) Drop();
    }

    void TryPickUp()
    {
        if (equippedWeapon != null) return; //already holding a weapon

        //raycast to find weapon
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

        //disable physics
        gunRb.useGravity = false;
        gunCol.enabled = false;
        outline.enabled = false;
        gunAnim.enabled = true;

        //attach to socket
        weapon.transform.DOMove(itemSocket.position, 0.2f).SetEase(Ease.OutQuad);
        weapon.transform.DORotateQuaternion(itemSocket.rotation, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            weapon.transform.SetParent(itemSocket);
            weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        });

        weapon.layer = LayerMask.NameToLayer("Clipping");

        //reset velocity
        gunRb.velocity = Vector3.zero;
        gunRb.angularVelocity = Vector3.zero;
    }

    void Drop()
    {
        if (equippedWeapon == null) return;

        //detach and enable physics
        equippedWeapon.transform.SetParent(null);
        gunRb.useGravity = true;
        gunCol.enabled = true;
        outline.enabled = true;
        gunAnim.enabled = false;

        equippedWeapon.layer = LayerMask.NameToLayer("Gun");

        //apply force + carry player's momentum
        gunRb.velocity = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>().velocity;
        gunRb.AddForce(Camera.main.transform.forward * dropForwardForce + Vector3.up * dropUpwardForce, ForceMode.Impulse);

        //clear references
        if (equippedWeapon.TryGetComponent<Gun>(out var gun))
            gun.enabled = false;

        equippedWeapon = null;
        gunRb = null;
        gunCol = null;
        gunAnim = null;
    }

    public void DockEquippedWeapon()
    {
        if (equippedWeapon == null) return;

        Transform gunTransform = equippedWeapon.transform;

        gunTransform.DOKill(); //cancel any active tweens

        Vector3 dockedPos = new(.3f, -.2f, -.4f);
        Vector3 dockedRot = new(-10f, 45f, -55f);

        float dockDuration = 5f / 60f;
        float resetDelay = 15f / 60f;

        gunTransform.DOLocalMove(dockedPos, dockDuration).SetEase(Ease.OutQuad);
        gunTransform.DOLocalRotate(dockedRot, dockDuration).SetEase(Ease.OutQuad);

        DOVirtual.DelayedCall(resetDelay, () =>
        {
            gunTransform.DOLocalMove(Vector3.zero, dockDuration).SetEase(Ease.OutQuad);
            gunTransform.DOLocalRotate(Vector3.zero, dockDuration).SetEase(Ease.OutQuad);
        });
    }
}