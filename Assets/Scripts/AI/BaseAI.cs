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
    [Range(1, 100)][SerializeField] int HP;         
    [Range(1, 180)][SerializeField] int viewCone;
    [Range(1, 100)][SerializeField] int targetFaceSpeed;

    [Header("------ Gunplay ------")]
    [Range(1, 100)][SerializeField] float shootRate;

    // defs
    bool istakingdamage;
    bool isShooting;
    bool playerInRange;
    float angleToPlayer;
    Vector3 playerDir;

    void Start()
    {
        HUDManager.instance.UpdateProgress(1);
        //agent.speed = moveSpeed;
    }

    void Update()
    {
        if ((playerInRange && canSeePlayer()) || !Investigate())
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

    bool Investigate() 
    {
        if (istakingdamage)
        {
            agent.SetDestination(GameManager.instance.player.transform.position);
            istakingdamage = false;
            return true;
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

    public void TakeDamage(int damage)
    {
        HP -= damage;
        StartCoroutine(flashRed());
        istakingdamage = true;

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
