using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseAI : MonoBehaviour, IDamageable
{
    [SerializeField] int HP; // health for the AI
    [SerializeField] int moveSpeed; // normal walk speed for the AI
    [SerializeField] int runSpeed; // sprintMod for the AI
    [SerializeField] float rotationSpeed; 
    [Range(5f, 15f)][SerializeField] int rangeOfWalkPoint;
    [SerializeField] int stoppingDist;
    [SerializeField] GameObject player;


    bool playerInRange;
    Vector3 playerDir;
    bool isMoving;
    NavMeshAgent agent;
    Transform destinatonPoint;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDist;
    }

    void Update()
    {    
        if (playerInRange)
        {
            isMoving = !(Vector3.Distance(transform.position, destinatonPoint.position) < 0.01f);
            Patrol();
        } 
    }

    IEnumerator Idle(float delay) // all AI will incorportate the same Idle behavior
    {
        yield return new WaitForSeconds(delay);
    }

    void Patrol() // all AI will incorpoate the same patrol behavior
    {
        if (!isMoving)
            StartCoroutine(Idle(5));
        agent.SetDestination(destinatonPoint.position);
    }


    public void TakeDamage(int damage) // all AI will take Damage, Or will there be various AI who would take different kinds of damage?
    {
        HP -= damage;

        if (HP <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator LocatorRandomizer()
    {
        yield return null;
    }

    bool canSeePlayer()
    {

        return false;
    }
}
