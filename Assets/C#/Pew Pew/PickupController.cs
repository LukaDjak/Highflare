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

        if (weapon.TryGetComponent<Gun>(out var gun))
            gun.enabled = true;

        //disable physics
        gunRb.useGravity = false;
        gunCol.enabled = false;

        //attach to socket
        weapon.transform.DOMove(itemSocket.position, 0.3f).SetEase(Ease.OutQuad);
        weapon.transform.DORotateQuaternion(itemSocket.rotation, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            weapon.transform.SetParent(itemSocket);
            weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        });

        gameObject.layer = LayerMask.NameToLayer("Clipping");

        //reset velocity
        gunRb.velocity = Vector3.zero;
        gunRb.angularVelocity = Vector3.zero;
    }

    void Drop()
    {
        if (equippedWeapon == null) return;

        gameObject.layer = LayerMask.NameToLayer("Gun");

        //detach and enable physics
        equippedWeapon.transform.SetParent(null);
        gunRb.useGravity = true;
        gunCol.enabled = true;

        //apply force + carry player's momentum
        gunRb.velocity = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>().velocity;
        gunRb.AddForce(Camera.main.transform.forward * dropForwardForce + Vector3.up * dropUpwardForce, ForceMode.Impulse);

        //clear references
        if (equippedWeapon.TryGetComponent<Gun>(out var gun))
            gun.enabled = false;

        equippedWeapon = null;
        gunRb = null;
        gunCol = null;
    }
}