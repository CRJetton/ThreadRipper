using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance { get; private set; }

    List<KillDoor> killDoorList = new List<KillDoor>();


    private void Awake()
    {
        instance = this; 
    }

    private void Start()
    {
        GameObject[] killDoors = GameObject.FindGameObjectsWithTag("KillDoor");

        foreach (GameObject killDoor in killDoors)
        {
            killDoorList.Add(killDoor.GetComponent<KillDoor>());
        }
    }

    public void KillDoorEnemySpawned(GameObject enemy)
    {
        foreach(KillDoor killDoor in killDoorList)
        {
            killDoor.AddEnemyToKill(enemy);
        }
    }
}
