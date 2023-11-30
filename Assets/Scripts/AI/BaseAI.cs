using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class BaseAI : MonoBehaviour, IDamageable
{
    [SerializeField] int HP; // health for the AI
    [SerializeField] int moveSpeed; // normal walk speed for the AI
    [SerializeField] int runSpeed; // sprintMod for the AI
    [SerializeField] int punchDamage; // The AI can Punch the player if he's without weapons
    [SerializeField] float rotationSpeed;
    [SerializeField] GameObject player;
    // [SerializeField] float rotationSpeed;


    int stoppingDist;
    bool playerInSight;
    bool isMoving;
    NavMeshAgent agent;
    Vector3 destinatonPoint;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        agent.speed = moveSpeed;
    }

    void Update()
    {
        playerInSight = Vector3.Distance(transform.position, player.transform.position) < 5;
    
        if (playerInSight)
        {
            // Start here
        }
        else
        {
            Patrol();
        }

    }
    // public abstract void Combat(); // Multiple AI's will derive off from different Combat styles

    IEnumerator Idle(float delay) // all AI will incorportate the same Idle behavior
    {
        yield return new WaitForSeconds(delay);
    }

    void Patrol() // all AI will incorpoate the same patrol behavior
    {
        // Code here        
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

    public abstract void Combat(); // Multiple AI's will derive off from different Combat styles
}
