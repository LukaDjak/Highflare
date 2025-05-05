using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator animator;

    private Collider mainCol;
    private Collider[] allColliders;
    private Rigidbody[] allRigidBodies;

    [HideInInspector] public bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        mainCol = GetComponent<Collider>();
        allColliders = GetComponentsInChildren<Collider>(true);
        allRigidBodies = GetComponentsInChildren<Rigidbody>(true);

        DoRagdoll(false);
    }

    public void DoRagdoll(bool isRagdoll)
    {
        //drop a gun if dead

        foreach (var col in allColliders)
            col.enabled = isRagdoll;
        foreach (var rb in allRigidBodies)
            rb.isKinematic = !isRagdoll;

        animator.enabled = !isRagdoll;
        mainCol.enabled = !isRagdoll;
        //toggle navmesh agent
    }
}
