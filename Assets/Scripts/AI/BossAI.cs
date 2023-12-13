using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

// Defines the AI behavior for the Boss character in our game.
public class BossAI: MonoBehaviour, IDamageable
{
    // Components attached to the boss character for navigation, rendering, and animation
    [Header("------ Components ------")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Renderer model;
    [SerializeField] private Transform headPosition;
    [SerializeField] public Animator animator;
    [SerializeField] Collider damageCollider;

    // Configurable stats for the boss character to effect the feel of the boss
    [Header("------ Boss Stats ------")]
    [Range(1, 10000)][SerializeField] private float HP;
    [Range(1, 360)]  [SerializeField] private int viewCone;
    [Range(1, 100)]  [SerializeField] private int targetFaceSpeed;
    [Range(1, 3)]    [SerializeField] private float weakSpotMultiplier;
    [Range(1, 3)]    [SerializeField] private float weakSpotThreshold;

    // Roaming behavior settings for when the boss is not engaging the player
    [SerializeField] int roamDistance;
    [SerializeField] int roamPauseTime;
    [SerializeField] float animationSpeedTransition;

    // Phase speeds to change the boss's movement and turning speed during different phases of the fight
    [Header("------ Phase Speeds ------")]
    [Range(1, 3)]   [SerializeField] private float phaseOneMoveSpeed;
    [Range(1, 3)]   [SerializeField] private float phaseTwoMoveSpeed;
    [Range(1, 3)]   [SerializeField] private float phaseThreeMoveSpeed;

    // Turn speed of the mech (changed phase one in order to fine tune it for sequential range increases)
    [SerializeField] private float phaseOneTurnSpeed;
    [Range(1, 3)]   [SerializeField] private float phaseTwoTurnSpeed;
    [Range(1, 3)]   [SerializeField] private float phaseThreeTurnSpeed;

    // Attack settings for the boss (can probably be removed, but left in for fine tunning by the level designers)
    [Header("----- Boss Attacks -----")]
    [Range(1, 100)] [SerializeField] private float shootRate;

    // Internal state variables to control AI behavior
    bool istakingdamage;
    bool isShooting = false;
    bool playerInRange;
    bool destinationChosen;

    // Variables to track the position and orientation of the boss relative to the player
    float angleToPlayer;
    float stoppingDistanceOrigional;
    float maxHP;

    // Direction and position tracking
    Vector3 playerDir;
    Vector3 startingPos;

    // Reference to the BossCombat script for combat functionalities
    private BossCombat bossCombat;

    // Initialization of boss's state at the start of the game
    #region initialization
    void Start()
    {
        // initial values and states
        HUDManager.instance.UpdateProgress(1);                                          // Updates the HUD to show the initial state of the boss
        startingPos = transform.position;                                               // Stores the boss's starting position for later use
        stoppingDistanceOrigional = agent.stoppingDistance;                             // Saves the original stopping distance of the navigation agent
        bossCombat = GetComponent<BossCombat>();                                        // Retrieves the BossCombat component attached to the boss
        if (bossCombat != null)                                                         // Initializes the max HP with the boss's starting HP for phase changes
        {
            bossCombat.InitializeMaxHP(HP);
        }
    }
    #endregion

    // Update is called once per frame to check and update boss's behavior
    #region update
    void Update()
    {
        // Update the boss's behavior based on the player's position and the boss's state
        if (agent.isActiveAndEnabled)
        {
            // Update animation speed based on the boss's movement
            float animatorSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", Mathf.Lerp(animator.GetFloat("Speed"), animatorSpeed, Time.deltaTime * animationSpeedTransition));

            // Check if the boss should roam or engage with the player
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

    // Defines the roaming behavior of the boss
    #region boss behavior
    IEnumerator roam()                                                                          
    {
        // Boss chooses a new destination to roam to if the current destination is reached (change number to change behavior)
        if (agent.remainingDistance < 0.05f && !destinationChosen)
        {
            destinationChosen = true;
            agent.stoppingDistance = 0;
            yield return new WaitForSeconds(roamPauseTime);

            // Selects a random position within its specified radius to roam to
            Vector3 randomPos = Random.insideUnitSphere * roamDistance;
            randomPos += startingPos;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomPos, out hit, roamDistance, 1);
            agent.SetDestination(hit.position);

            destinationChosen = false;
        }
    }

    // Checks if the boss can see the player based on vision cone and line of sight
    bool canSeePlayer()
    {
        // Calculate the direction and angle to the player
        playerDir = GameManager.instance.playerBodyPositions.GetPlayerCenter() - headPosition.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        // Visualize the line of sight for debugging (can be removed if no longer necesary)
        Debug.DrawRay(headPosition.position, playerDir);

        
        RaycastHit hit;

        // Perform a raycast to detect if the player is within line of sight
        if (Physics.Raycast(headPosition.position, playerDir, out hit))
        {
            // If the player is detected within the vision cone, engage in combat
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewCone)
            {
                agent.SetDestination(GameManager.instance.player.transform.position);
                if (bossCombat != null)
                {
                    bossCombat.AimAtPlayer(GameManager.instance.playerBodyPositions.GetPlayerCenter());
                }

                // Initiate shooting sequence if the player is within range and not already shooting
                if (!isShooting)
                {
                    shoot();
                }

                // Make the boss face the target if within stopping distance
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    faceTarget();
                }
                agent.stoppingDistance = stoppingDistanceOrigional;
                return true;
            }
        }
        // Reset the stopping distance if the player is not in sight
        agent.stoppingDistance = 0;
        return false;
    }

    // Investigate the last known position of the player if the boss is taking damage
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

    // Method to orient the boss to face the player
    void faceTarget()
    {
        // Calculate the rotation required to face the player
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDir.x, transform.position.y, playerDir.z));

        // Smoothly rotate towards the player
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * targetFaceSpeed);
    }

    // Used by the animator to adjust the boss's look direction based on the player's position
    void OnAnimatorIK(int layerIndex)
    {
        if (playerInRange)
        {
            Vector3 lookTargetPosition = GameManager.instance.player.transform.position;

            // Set the boss's gaze to follow the player
            animator.SetLookAtWeight(1f); 
            animator.SetLookAtPosition(lookTargetPosition);
        }
    }

    // Triggered when a collider enters the boss's trigger zone
    public void OnTriggerEnter(Collider other)
    {
        // Check if the player has entered the trigger zone
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    // Adjusts the boss's speed based on the current combat phase
    public void UpdateSpeed(int phase)
    {
        // Store base movement and turning speeds
        float baseMoveSpeed = agent.speed;
        float baseTurnSpeed = targetFaceSpeed;

        // Adjust speed based on the phase of the battle
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

    // Defines combat behaviors, including shooting at the player
    #region combat
    private void shoot()
    {
        // Initiate shooting if the boss is not already doing so
        if (!isShooting && bossCombat != null)
        {
            StartCoroutine(ShootSequence());
        }
    }

    // Coroutine to handle the shooting sequence
    private IEnumerator ShootSequence()
    {

        isShooting = true;

        // Call the method to fire weapons
        bossCombat.FireWeapons();

        // Wait for a period based on the shooting rate before allowing the next shot (may not be needed)
        yield return new WaitForSeconds(shootRate);

        isShooting = false;

    }

    // Method to handle the boss taking damage
    public void TakeDamage(float damage)
    {
        // Calculate the direction and angle to the player for weak spot calculation 
        Vector3 toPlayer = (GameManager.instance.player.transform.position - transform.position).normalized;
        float angleToWeakSpot = Vector3.Angle(-transform.forward, toPlayer);
        float weakSpotConeAngle = 30f;                                                      // Change this value to effect the cone angle for extra damage
        if (angleToWeakSpot <= weakSpotConeAngle)
        {
            damage *= weakSpotMultiplier;                                                   // Weak spot multiplier is changed in unity
        }

        HP -= damage;
        // StartCoroutine(flashRed()); (currently commented out because it changes the colors in the scene aswell)
        istakingdamage = true;

        // Changes phase for health threshold
        if (bossCombat != null)
        {
            bossCombat.ChangePhase(HP);
        }

        // Handles the boss death and runs his death animation
        if (HP <= 0)
        {
            HUDManager.instance.UpdateProgress(-1);
            animator.SetBool("Dead", true);
            agent.enabled = false;
            damageCollider.enabled = false;
        }
        else
        {
            StopAllCoroutines();
            isShooting = false;
            destinationChosen = false;
            // StartCoroutine(flashRed());
            agent.SetDestination(GameManager.instance.player.transform.position);
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