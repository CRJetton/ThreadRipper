using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//[RequireComponent(typeof(NavMeshAgent))]
public class BaseAI : MonoBehaviour, IDamageable
{
    // Behaviors and stats
    [Header("------ Components ------")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform headPosition;
    [SerializeField] EnemyCombat enemyCombat;

    [Header("------ Enemy Stats ------")]
    [Range(1, 100)][SerializeField] int HP;               // health for the AI
    //[Range(1, 100)][SerializeField] int moveSpeed;        // normal walk speed for the AI
    //[Range(1, 100)][SerializeField] int runSpeed;         // sprintMod for the AI
    [Range(1, 180)][SerializeField] int viewCone;
    [Range(1, 100)][SerializeField] int targetFaceSpeed;

    [Header("------ Gunplay ------")]
    [Range(1, 100)][SerializeField] float shootRate;
    
    //[SerializeField] GameObject bullet;
    //[SerializeField] Transform shootPosition;

    // add in call aimAt function, attackStarted to start shooting, attackCancel to stop and reload to reload

    // defs
    bool isShooting;
    bool playerInRange;
    float angleToPlayer;
    Vector3 playerDir;

    // Needs to be built into the game manager, remove later
    //[SerializeField] GameObject player;
    // [SerializeField] float rotationSpeed;

    // patrolling 
    bool isMoving;
    bool playerInSight;
    int stoppingDist;
    Vector3 destinatonPoint;

    //void Awake()
    //{
    //    agent = GetComponent<NavMeshAgent>();
    //}

    void Start()
    {
        HUDManager.instance.UpdateProgress(1);
        //player = GameManager.instance.player;
        //agent.speed = moveSpeed;
    }

    void Update()
    {
        if (playerInRange && canSeePlayer())
        {
            // Patrol();
        }
    }

    bool canSeePlayer()
    {
        playerDir = GameManager.instance.playerBodyPositions.playerCenter.position - headPosition.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(headPosition.position, playerDir);

        RaycastHit hit;

        if (Physics.Raycast(headPosition.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewCone)
            {
                agent.SetDestination(GameManager.instance.player.transform.position);
                enemyCombat.AimAt(GameManager.instance.playerBodyPositions.playerCenter.position);

                if (!isShooting)
                {
                    StartCoroutine(shoot());
                }

                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    faceTarget();
                }

                return true;
            }
        }

        return false;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * targetFaceSpeed);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    IEnumerator shoot()
    {
        isShooting = true;

        enemyCombat.AttackStarted();

        yield return new WaitForSeconds(shootRate);
        enemyCombat.AttackCanceled();

        isShooting = false;
    }

    public void TakeDamage(int damage)              // all AI will take Damage, Or will there be various AI who would take different kinds of damage?
    {
        HP -= damage;
        StartCoroutine(flashRed());

        if (HP <= 0)
        {
            Destroy(gameObject);
            HUDManager.instance.UpdateProgress(-1);
            
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }

    // public abstract void Combat(); // Multiple AI's will derive off from different Combat styles

    //IEnumerator Idle(float delay) // all AI will incorportate the same Idle behavior
    //{
    //    yield return new WaitForSeconds(delay);
    //}

    //void Patrol() // all AI will incorpoate the same patrol behavior
    //{
    //    // Code here        
    //}

    //IEnumerator LocatorRandomizer()
    //{
    //    yield return null;
    //}

    //bool canSeePlayer()
    //{
    //    return false;
    //}


}
