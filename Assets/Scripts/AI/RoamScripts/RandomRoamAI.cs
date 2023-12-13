using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RandomRoamAI : BaseAI
{
    [Header("----- Random Roam Component -----")]
    [SerializeField] int roamDist;
    [SerializeField] int roamPause;

    bool destinationChoosen;
    Vector3 startingPos;
    Coroutine randomRoamcot;

    private void Start()
    {
        EnemyManager.instance.enemies.Add(gameObject);
        destinationChoosen = false;
        startingPos = transform.position;
        HUDManager.instance.UpdateProgress(1);
    }

    public override void patrol()
    {
        agent.stoppingDistance = 0;
        randomRoamcot = StartCoroutine(RandomPoint(roamPause));
    }

    public override bool canSeePlayer()
    {
        if (base.canSeePlayer())
        {
            destinationChoosen = false;
            if (randomRoamcot != null)
                StopCoroutine(randomRoamcot);
            return true;
        }

        return false;
    }

    IEnumerator RandomPoint(int delay)
    {
        if (agent.isOnNavMesh)
        {
            if (agent.remainingDistance <= 0.01f && !destinationChoosen)
            {
                destinationChoosen = true;
                agent.stoppingDistance = 0;
                yield return new WaitForSeconds(delay);

                Vector3 randPos = Random.insideUnitSphere * roamDist;
                randPos += startingPos;

                NavMeshHit hit;
                NavMesh.SamplePosition(randPos, out hit, roamDist, 1);
                agent.SetDestination(hit.position);

                destinationChoosen = false;
            }
        }
    }

    public override void TakeDamage(float damage)
    {
        destinationChoosen = false;
        if (randomRoamcot != null)
            StopCoroutine(randomRoamcot);
        base.TakeDamage(damage);
    }
}
