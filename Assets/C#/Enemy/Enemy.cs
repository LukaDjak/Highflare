using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 2f;
    [SerializeField] private float shootCooldown = 2f;
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float idleWaitTime = 2f;

    [Header("Range")]
    [SerializeField] private float patrolRange = 15f;
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float shootRange = 4f;

    public bool isFriendly = false;
    public bool shouldMove = true;

    private float chaseSpeed;
    private float lastShotTime;
    private float idleTimer;
    private Transform player;
    private Animator animator;
    private Collider mainCol;
    private Collider[] allColliders;
    private Rigidbody[] allRigidBodies;
    private WeaponDropper dropper;
    private NavMeshAgent agent;
    private Vector3 startPosition;

    private float firstShotDelay = 0f;
    private float shootStateEnterTime = 0f;

    private enum State { Idle, Patrol, Chase, Shoot }
    private State currentState;

    [HideInInspector] public bool isDead = false;

    private void Awake() => player = GameObject.FindGameObjectWithTag("Player").transform;

    private void Start()
    {
        animator = GetComponent<Animator>();
        mainCol = GetComponent<Collider>();
        allColliders = GetComponentsInChildren<Collider>(true);
        allRigidBodies = GetComponentsInChildren<Rigidbody>(true);
        dropper = GetComponent<WeaponDropper>();
        agent = GetComponent<NavMeshAgent>();

        chaseSpeed = baseSpeed * 1.5f;
        startPosition = transform.position;

        DoRagdoll(false);
    }

    private void Update()
    {
        if (isDead || !shouldMove) return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= shootRange)
            SetState(State.Shoot);
        else if (distanceToPlayer <= chaseRange)
            SetState(State.Chase);
        else
            SetState(State.Patrol);

        UpdateState();
    }

    private void SetState(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (newState)
        {
            case State.Idle:
                animator.SetInteger("State", 0); //idle anim
                agent.isStopped = true;
                break;

            case State.Patrol:
                animator.SetInteger("State", 1); //run anim
                agent.speed = baseSpeed;
                agent.isStopped = false;
                idleTimer = idleWaitTime; //force immediate move on entering patrol
                break;

            case State.Chase:
                animator.SetInteger("State", 1); //run anim
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                break;

            case State.Shoot:
                animator.SetInteger("State", 2); //shoot idle anim
                agent.isStopped = true;

                //initialize delay timer on entering shooting state
                firstShotDelay = Random.Range(0.5f, 1f);
                shootStateEnterTime = Time.time;

                lastShotTime = 0f; //reset last shot so cooldown triggers after delay
                break;
        }
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case State.Patrol:
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    idleTimer += Time.deltaTime;
                    if (idleTimer >= idleWaitTime)
                    {
                        MoveToRandomPoint();
                        idleTimer = 0f;
                    }
                    animator.SetInteger("State", 0);
                }
                else
                    animator.SetInteger("State", 1);
                break;

            case State.Chase:
                agent.SetDestination(player.position);
                FaceTarget(player.position);
                break;

            case State.Shoot:
                agent.isStopped = true;
                FaceTarget(player.position);
                animator.SetInteger("State", 2); // shoot

                //wait for first shot delay
                if (Time.time - shootStateEnterTime >= firstShotDelay)
                {
                    if (Time.time - lastShotTime >= shootCooldown)
                    {
                        ShootAtPlayer();
                        lastShotTime = Time.time;
                    }
                }
                break;
        }
    }

    private void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += startPosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void ShootAtPlayer()
    {
        Gun g = GetComponentInChildren<Gun>();
        if (g != null)
            g.EnemyShoot(player);
    }

    private void FaceTarget(Vector3 target)
    {
        Vector3 lookDir = (target - transform.position).normalized;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }

    public void DoRagdoll(bool isRagdoll)
    {
        //drop a gun if dead
        if (dropper.equippedWeapon != null)
            dropper.DropWeapon();

        foreach (var col in allColliders)
            col.enabled = isRagdoll;
        foreach (var rb in allRigidBodies)
            rb.isKinematic = !isRagdoll;

        dropper.enabled = !isRagdoll;
        animator.enabled = !isRagdoll;
        mainCol.enabled = !isRagdoll;
        agent.enabled = !isRagdoll;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRange);        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        if(!isFriendly)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, shootRange);
        }
    }
}