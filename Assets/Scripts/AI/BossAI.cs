using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BossAI: MonoBehaviour, IDamageable
{
    [Header("------ Components ------")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Renderer model;
    [SerializeField] private Transform headPosition;
    [SerializeField] public Animator animator;
    [SerializeField] Collider damageCollider;

    [Header("------ Boss Stats ------")]
    [Range(1, 10000)][SerializeField] private float HP;
    [Range(1, 360)]  [SerializeField] private int viewCone;
    [Range(1, 100)]  [SerializeField] private int targetFaceSpeed;
    [Range(1, 3)]    [SerializeField] private float weakSpotMultiplier;
    [Range(1, 3)]    [SerializeField] private float weakSpotThreshold;

    [SerializeField] int roamDistance;
    [SerializeField] int roamPauseTime;
    [SerializeField] float animationSpeedTransition;

    [Header("------ Phase Speeds ------")]
    [Range(1, 3)]   [SerializeField] private float phaseOneMoveSpeed;
    [Range(1, 3)]   [SerializeField] private float phaseTwoMoveSpeed;
    [Range(1, 3)]   [SerializeField] private float phaseThreeMoveSpeed;

    [Range(1, 3)]   [SerializeField] private float phaseOneTurnSpeed;
    [Range(1, 3)]   [SerializeField] private float phaseTwoTurnSpeed;
    [Range(1, 3)]   [SerializeField] private float phaseThreeTurnSpeed;

    [Header("----- Boss Attacks -----")]
    [Range(1, 100)] [SerializeField] private float shootRate;

    bool istakingdamage;
    bool isShooting = false;
    bool playerInRange;
    bool destinationChosen;

    float angleToPlayer;
    float stoppingDistanceOrigional;
    float maxHP;

    Vector3 playerDir;
    Vector3 startingPos;

    private BossCombat bossCombat;

    #region initialization
    void Start()
    {
        // initialization
        HUDManager.instance.UpdateProgress(1);
        startingPos = transform.position;
        stoppingDistanceOrigional = agent.stoppingDistance;
        bossCombat = GetComponent<BossCombat>();
        if (bossCombat != null)
        {
            bossCombat.InitializeMaxHP(HP);
        }
    }
    #endregion

    #region update
    void Update()
    {
        if (agent.isActiveAndEnabled)
        {
            float animatorSpeed = agent.velocity.magnitude;

            animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), animatorSpeed, Time.deltaTime * animationSpeedTransition));

            if (playerInRange && !canSeePlayer() || !Investigate())
            {
                StartCoroutine(roam());
            }
            else if (!playerInRange)
            {
                StartCoroutine(roam());
            }
        }
    }
    #endregion

    #region boss behavior
    IEnumerator roam()                                                                          
    {
        if (agent.remainingDistance < 0.05f && !destinationChosen)
        {
            destinationChosen = true;
            agent.stoppingDistance = 0;
            yield return new WaitForSeconds(roamPauseTime);

            Vector3 randomPos = Random.insideUnitSphere * roamDistance;
            randomPos += startingPos;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomPos, out hit, roamDistance, 1);
            agent.SetDestination(hit.position);

            destinationChosen = false;
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
                if (bossCombat != null)
                {
                    bossCombat.AimAtPlayer(GameManager.instance.playerBodyPositions.playerCenter.position);
                }

                if (!isShooting)
                {
                    shoot();
                }

                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    faceTarget();
                }
                agent.stoppingDistance = stoppingDistanceOrigional;
                return true;
            }
        }
        agent.stoppingDistance = 0;
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
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * targetFaceSpeed);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (playerInRange)
        {
            Vector3 lookTargetPosition = GameManager.instance.player.transform.position;

            animator.SetLookAtWeight(1f); 
            animator.SetLookAtPosition(lookTargetPosition);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    public void UpdateSpeed(int phase)
    {
        float baseMoveSpeed = agent.speed;
        float baseTurnSpeed = targetFaceSpeed;

        switch (phase)
        {
            case 1:
                agent.speed = baseMoveSpeed * phaseOneMoveSpeed;
                targetFaceSpeed = (int)(baseTurnSpeed * phaseOneTurnSpeed);
                break;
            case 2:
                agent.speed = baseMoveSpeed * phaseTwoMoveSpeed;
                targetFaceSpeed = (int)(baseTurnSpeed * phaseTwoTurnSpeed);
                break;
            case 3:
                agent.speed = baseMoveSpeed * phaseThreeMoveSpeed;
                targetFaceSpeed = (int)(baseTurnSpeed * phaseThreeTurnSpeed);
                break;
        }
    }
    #endregion

    #region combat
    private void shoot()
    {
        if (!isShooting && bossCombat != null)
        {
            StartCoroutine(ShootSequence());
        }
    }

    private IEnumerator ShootSequence()
    {

        isShooting = true;
        
        bossCombat.FireWeapons();
        
        yield return new WaitForSeconds(shootRate);

        isShooting = false;

    }

    public void TakeDamage(float damage)
    {
        Vector3 toPlayer = (GameManager.instance.player.transform.position - transform.position).normalized;
        float angleToWeakSpot = Vector3.Angle(-transform.forward, toPlayer);
        float weakSpotConeAngle = 30f;
        if (angleToWeakSpot <= weakSpotConeAngle)
        {
            damage *= weakSpotMultiplier;
        }

        HP -= damage;
        // StartCoroutine(flashRed());
        istakingdamage = true;

        if (bossCombat != null)
        {
            bossCombat.ChangePhase(HP);
        }
        
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
#endregion