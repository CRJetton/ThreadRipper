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
    bool startSpawning;
    List<Transform> usableSpawnPos = new List<Transform>();


    private void Start()
    {
        usableSpawnPos.AddRange(spawnPos);
    }


    private void Update()
    {
        if (startSpawning && spawnCount < numToSpawn && !isSpawning)
        {
            StartCoroutine(Spawn());
        }
    }


    void EnemyDied()
    {
        numToSpawn--;
        spawnCount--;
    }



    IEnumerator Spawn()
    {
        isSpawning = true;

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

        isSpawning = false;
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        if (other.CompareTag("Player"))
        {
            startSpawning = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
            return;

        if (other.CompareTag("Player"))
        {
            startSpawning = false;
        }
    }
}
