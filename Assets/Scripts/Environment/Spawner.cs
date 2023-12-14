using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] bool spawnForKillDoor;
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] int numToSpawn;
    [SerializeField] float timeBetweenSpawns;
    [SerializeField] Transform[] spawnPos;
    [SerializeField] List<GameObject> spawnList = new List<GameObject>();

    int spawnCount;
    bool isSpawning;
    List<Transform> usableSpawnPos = new List<Transform>();


    private void Start()
    {
        HUDManager.instance.UpdateProgress(numToSpawn);

        usableSpawnPos.AddRange(spawnPos);
    }


    void EnemyDied()
    {
        numToSpawn--;
        spawnCount--;
    }



    IEnumerator Spawn()
    {
        isSpawning = true;

        for(int i = 0; i < numToSpawn; i++)
        {
            int posIndex = Random.Range(0, usableSpawnPos.Count - 1);
            GameObject objectClone = Instantiate(objectToSpawn, usableSpawnPos[posIndex].position, usableSpawnPos[posIndex].rotation);

            objectClone.GetComponent<BaseAI>().SubscribeOnDie(EnemyDied);

            spawnList.Add(objectClone);
            spawnCount++;

            if (spawnForKillDoor)
                SpawnManager.instance.KillDoorEnemySpawned(objectClone);

            usableSpawnPos.RemoveAt(posIndex);

            if (usableSpawnPos.Count == 0)
                usableSpawnPos.AddRange(spawnPos);

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        if (other.CompareTag("Player") && !isSpawning)
        {
            StartCoroutine(Spawn());
        }
    }
}
