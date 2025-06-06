using UnityEngine;

public class WeaponDropper : MonoBehaviour
{
    [Header("References")]
    public Transform weaponSocket;

    [Header("Drop Settings")]
    public float dropForwardForce = 2f;
    public float dropUpwardForce = 1.5f;

    [HideInInspector] public GameObject equippedWeapon;
    private Rigidbody gunRb;
    private Collider gunCol;
    private Animator gunAnim;
    private QuickOutline outline;

    void Start()
    {
        if (weaponSocket.childCount == 0) return;

        equippedWeapon = weaponSocket.GetChild(0).gameObject;

        gunRb = equippedWeapon.GetComponent<Rigidbody>();
        gunCol = equippedWeapon.GetComponent<Collider>();
        gunAnim = equippedWeapon.GetComponent<Animator>();
        outline = equippedWeapon.GetComponent<QuickOutline>();

        if (equippedWeapon.TryGetComponent<Gun>(out var gun))
            gun.enabled = true;

        gunRb.useGravity = false;
        gunCol.enabled = false;
        if (outline != null) outline.enabled = false;
        gunAnim.enabled = true;
    }

    public void DropWeapon()
    {
        if (equippedWeapon == null) return;

        equippedWeapon.transform.SetParent(null);

        gunRb.useGravity = true;
        gunCol.enabled = true;
        outline.enabled = true;
        gunAnim.enabled = false;

        equippedWeapon.layer = LayerMask.NameToLayer("Gun");

        Vector3 dropForce = transform.forward * dropForwardForce + Vector3.up * dropUpwardForce;
        gunRb.AddForce(dropForce, ForceMode.Impulse);

        if (equippedWeapon.TryGetComponent<Gun>(out var gun))
            gun.enabled = false;

        equippedWeapon = null;
        gunRb = null;
        gunCol = null;
        gunAnim = null;

        this.enabled = false;
    }
}