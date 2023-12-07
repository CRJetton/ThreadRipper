using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointRoamAI : BaseAI
{
    [Header("----- Waypoint Roam Requirements -----")]
    [SerializeField] Transform waypoint;
    [SerializeField] int roamPause;

    public bool walkpointSet; // public for debug reasons

    int currentWaypointIndex;
    Transform currentWaypoint;

    // Start is called before the first frame update
    void Start()
    {
        walkpointSet = false;
        currentWaypointIndex = 0;
        transform.position = waypoint.GetChild(0).position;
        HUDManager.instance.UpdateProgress(1);
    }

    public override void patrol()
    {
        agent.stoppingDistance = 0;
        StartCoroutine(WaypointPatrol(roamPause));
    }

    public IEnumerator WaypointPatrol(int delay)
    {
        if (agent.remainingDistance < 0.01f && !walkpointSet)
        {
            walkpointSet = true;
            agent.stoppingDistance = 0;
            yield return new WaitForSeconds(delay);
            if (currentWaypointIndex <= waypoint.childCount - 1)
            {
                currentWaypoint = waypoint.GetChild(currentWaypointIndex);
                currentWaypointIndex++;
            }
            else
            {
                currentWaypoint = waypoint.GetChild(0);
                currentWaypointIndex = 0;
            }
            agent.SetDestination(currentWaypoint.position);
            walkpointSet = false;
        }
    }

    public override void TakeDamage(float damage)
    {
        walkpointSet = false;
        base.TakeDamage(damage);
    }
}