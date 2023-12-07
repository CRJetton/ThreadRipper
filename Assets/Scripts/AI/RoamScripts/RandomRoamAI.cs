using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomRoamAI : BaseAI, IDamageable
{
    [Header("----- Random Roam Component -----")]
    [SerializeField] int roamDist;
    
    public bool destinationChoosen;
    Vector3 startingPos;

    private void Start()
    {
        destinationChoosen = false;
        startingPos = transform.position;
        HUDManager.instance.UpdateProgress(1);
    }

    public override void patrol()
    {
        StartCoroutine(RandomPoint(1));
    }

    IEnumerator RandomPoint(int delay)
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

    public void TakeDamage(float damage)
    {
        destinationChoosen = false;
        base.TakeDamage(damage);
    }
}
