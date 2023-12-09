using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


// NOTE TO OURSELVES:
// Contain most of these information through OOP (if possible)
// - Encapsulation
// - Inheritance
// Create a functionality where the Enemy can vault over vaultable objects.

public class BaseAI : MonoBehaviour, IDamageable
{
    // Behaviors and stats
    [Header("------ Components ------")]
    [SerializeField] public NavMeshAgent agent; // READ --> Keep it public so the roam scripts can access this variable
    [SerializeField] Renderer model;
    [SerializeField] Transform headPosition;
    [SerializeField] EnemyCombat enemyCombat;
    [SerializeField] float stoppingDist;

    [Header("------ Enemy Stats ------")]
    [Range(1, 100)][SerializeField] float HP;
    [Range(1, 180)][SerializeField] int viewCone;
    [Range(1, 100)][SerializeField] int targetFaceSpeed;

    [Header("------ Gunplay ------")]
    [Range(1, 100)][SerializeField] float shootRate;

    // defs
    bool isShooting;
    bool playerInRange;
    float angleToPlayer;
    Vector3 playerDir;

    //patrol
    bool isMoving;
    Vector3 enemyPos;
    Vector3 destinationPoint;
    Coroutine cot;

    void Start()
    {
        isMoving = false;
        HUDManager.instance.UpdateProgress(1);
        enemyPos = transform.position;
        stoppingDist = agent.stoppingDistance;
        //agent.speed = moveSpeed;
    }

    void Update()
    {
        if (playerInRange && !canSeePlayer())
        {
            patrol();
        }
        else if (!playerInRange)
        {
            patrol();
        }
    }

    public virtual void patrol()
    {
        if (isMoving)
        {
            agent.stoppingDistance = 0;
            cot = StartCoroutine(returntoLastPos(1));
        }
    }

    IEnumerator returntoLastPos(int delay)
    {
        if (agent.remainingDistance <= 0.01f)
        {
            destinationPoint = enemyPos;
            yield return new WaitForSeconds(delay);
            agent.SetDestination(destinationPoint);
            isMoving = false;
        }
    }

    public virtual bool canSeePlayer()
    {
        playerDir = GameManager.instance.playerBodyPositions.playerCenter.position - headPosition.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(headPosition.position, playerDir);

        RaycastHit hit;

        if (Physics.Raycast(headPosition.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewCone)
            {
                isShooting = false;
                if (cot != null)
                    StopCoroutine(cot);
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
                
                isMoving = true;
                agent.stoppingDistance = stoppingDist;
                return true;
            }
        }
        agent.stoppingDistance = 0;
        return false;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
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

    public virtual void TakeDamage(float damage)
    {
        HP -= damage;

        isShooting = false;
        if (cot != null)
            StopCoroutine(cot);
        StartCoroutine(flashRed());

        agent.SetDestination(GameManager.instance.player.transform.position);
        isMoving = true;

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
}
