using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;



public class BaseAI : MonoBehaviour, IDamageable
{
    // Behaviors and stats
    [Header("------ Enemy Stats ------")]
    [Range(1, 100)][SerializeField] float HP;
    [Range(1, 180)][SerializeField] int viewCone;
    [Range(1, 100)][SerializeField] int targetFaceSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintMultipler;

    [Header("------ Combat ------")]
    [Range(1, 100)][SerializeField] float shootRate;
    [SerializeField] EnemyCombat enemyCombat;

    [Header("------ NavMesh Components ------")]
    [SerializeField] public NavMeshAgent agent; // READ --> Keep it public so the roam scripts can access this variable
    [SerializeField] float stoppingDist;
    [SerializeField] bool isASniper;
    [SerializeField] int findNearestEnemies;

    [Header("------ Colliders ------")]
    [SerializeField] Collider hitCol;
    [SerializeField] Collider detectCol;

    [Header("------ Model ------")]
    [SerializeField] Renderer model;
    [SerializeField] Transform headPosition;


    [Header("------ Animation ------")]
    [SerializeField] Animator enemyAnim;
    [SerializeField] float animSpeedTransition;



    // private Combat variables
    bool isSprinting;
    bool isShooting;
    float angleToPlayer;
    Vector3 playerDir;

    //private Patrol variables
    bool playerInRange;
    bool isMoving;
    Vector3 enemyPos;
    Vector3 destinationPoint;
    Coroutine cot;

    void Start()
    {
        EnemyManager.instance.enemies.Add(gameObject);
        isMoving = false;
        HUDManager.instance.UpdateProgress(1);
        enemyPos = transform.position;
        agent.stoppingDistance = stoppingDist;
        agent.speed = walkSpeed;
    }

    void Update()
    {
        if (agent.isActiveAndEnabled)
        {
            float animVelocity = agent.velocity.normalized.magnitude;
            enemyAnim.SetFloat("Speed", Mathf.Lerp(enemyAnim.GetFloat("Speed"), animVelocity, Time.deltaTime * animSpeedTransition));


            if (playerInRange && !canSeePlayer())
            {
                patrol();
            }
            else if (!playerInRange)
            {
                patrol();
            }
        }
    }

    public virtual void patrol()
    {
        if (isMoving)
        {
            agent.stoppingDistance = 0;
            cot = StartCoroutine(returntoOriginPos(1));
        }
    }

    public virtual bool canSeePlayer()
    {
        playerDir = -headPosition.position + GameManager.instance.playerBodyPositions.playerCenter.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(headPosition.position, playerDir);

        RaycastHit hit;

        if (Physics.Raycast(headPosition.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewCone)
            {
                if (cot != null)
                    StopCoroutine(cot);
                if (!isASniper)
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
                if (!isSprinting)
                {
                    agent.speed *= sprintMultipler;
                    isSprinting = true;
                }
                agent.stoppingDistance = stoppingDist;
                return true;
            }
        }
        isSprinting = false;
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0;
        return false;
    }

    IEnumerator returntoOriginPos(int delay)
    {
        if (agent.remainingDistance <= 0.01f)
        {
            destinationPoint = enemyPos;
            yield return new WaitForSeconds(delay);
            agent.SetDestination(destinationPoint);
            isMoving = false;
        }
    }


    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, 0, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * targetFaceSpeed);
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

        if (HP <= 0)
        {
            hitCol.enabled = false;
            detectCol.enabled = false;
            agent.enabled = false;
            Destroy(gameObject);
            HUDManager.instance.UpdateProgress(-1);
            OnDie.Invoke();
            EnemyManager.instance.enemies.Remove(gameObject);
        }
        else
        {
            isShooting = false;
            if (cot != null)
                StopCoroutine(cot);
            StartCoroutine(flashRed());
            if (!isASniper)
            {
                agent.SetDestination(GameManager.instance.player.transform.position);
                isMoving = true;
            }
            else if (isASniper)
            {
                CallForBackup();
            }
        }
    }

    void CallForBackup()
    {
        for (int i = 0; i < EnemyManager.instance.enemies.Count; i++)
        {
            if (EnemyManager.instance.enemies[i] != null)
            {
                if (Vector3.Distance(transform.position, EnemyManager.instance.enemies[i].transform.position) < findNearestEnemies)
                {
                    BaseAI tempEnemy = EnemyManager.instance.enemies[i].GetComponent<BaseAI>();

                    if (tempEnemy != null)
                    {
                        if (!tempEnemy.isASniper && tempEnemy != this)
                        {
                            tempEnemy.agent.SetDestination(GameManager.instance.player.transform.position);
                            tempEnemy.isMoving = true;
                        }
                    }
                }
            }

        }
    }


    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = Color.white;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, findNearestEnemies);
    }

    private UnityEvent OnDie = new UnityEvent();

    public void SubscribeOnDie(UnityAction action) { OnDie.AddListener(action); }
    public void UnsubscribeOnDie(UnityAction action) { OnDie.RemoveListener(action); }
}
