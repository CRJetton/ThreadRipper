using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class AbstractAI : MonoBehaviour, IDamageable
{
    [Header("-------- Stats --------")]
    [SerializeField] int HP;
    [SerializeField] int moveSpeed;
    [SerializeField] int runSpeed;
    [SerializeField] int punchDamage;
    [SerializeField] int viewCone;
    [SerializeField] float stoppingDist;
    [SerializeField] float rotationSpeed;
    [Range(5f, 15f)][SerializeField] int enemySightRange;
    [Range(3f, 10f)][SerializeField] int rangeToAttack;


    NavMeshAgent agent;
    Vector3 destinationPoint;
    Vector3 EnemyPos;

    Vector3 playerDir;
    float angleToPlayer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        EnemyPos = transform.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDist;
        destinationPoint = transform.position;
        Debug.Log("Start");
    }

    // Update is called once per frame
    public void Update()
    {
        Debug.Log("Update");
        if (!canSeePlayer())
        {
            Patrol();
        }
    }

    IEnumerator LocatorRandomizer(float delay)
    {
        float randX = Random.Range(EnemyPos.x + 5f, EnemyPos.x + enemySightRange);
        float randZ = Random.Range(EnemyPos.z + 5f, EnemyPos.z + enemySightRange);

        Vector3 newLoc = new Vector3(randX, 0, randZ);
        yield return new WaitForSeconds(delay);
        destinationPoint = newLoc;
    }

    void Patrol()
    {
        agent.SetDestination(destinationPoint);
        if (agent.remainingDistance < agent.stoppingDistance)
        {
            Coroutine cot = StartCoroutine(LocatorRandomizer(5));
        }
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;

        if (HP <= 0)
        {
            Destroy(gameObject);
            HUDManager.instance.UpdateProgress(-1);
        }
    }

    bool canSeePlayer()
    {
        playerDir = GameManager.instance.player.transform.position - transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(transform.position, playerDir);
        Debug.Log(angleToPlayer);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewCone)
            {
                agent.SetDestination(GameManager.instance.player.transform.position);
                if (agent.remainingDistance < rangeToAttack)
                    Combat();
                else if (agent.remainingDistance < agent.stoppingDistance)
                    faceTarget();

                return true;
            }
        }


        return false;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemySightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeToAttack);
    }

    public abstract void Combat();

    public void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(GameManager.instance.player.transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
    }
}
