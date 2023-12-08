using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject[] waypoints;    
    public static EnemyManager instance;

    private void Awake()
    {
        instance = this;
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
    }
}
