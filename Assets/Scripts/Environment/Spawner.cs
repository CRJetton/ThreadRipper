using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] int numToSpawn;
    [SerializeField] int timeBetweenSpawns;
    [SerializeField] Transform[] spawnPos;
    [SerializeField] List<GameObject> spawnList = new List<GameObject>();

    [SerializeField] bool spawnForKillDoor;

    int spawnCount;
    bool isSpawning;
    bool startSpawning;


    private void Start()
    {
    }


    private void Update()
    {
        if (startSpawning && spawnCount < numToSpawn && !isSpawning)
        {
            StartCoroutine(Spawn());
        }
    }


    public void EnemyDied()
    {
        numToSpawn--;
        spawnCount--;
        GameManager.instance.UpdateGameGoal(-1);
    }



    IEnumerator Spawn()
    {
        isSpawning = true;

        int posIndex = Random.Range(0, spawnPos.Length - 1);
        GameObject objectClone = Instantiate(objectToSpawn, spawnPos[posIndex].position, spawnPos[posIndex].rotation);

        objectClone.GetComponent<EnemyAI>().mySpawner = this;

        spawnList.Add(objectClone);
        spawnCount++;

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

            for (int i = 0; i < spawnList.Count; i++)
            {
                Destroy(spawnList[i]);
            }

            spawnList.Clear();
            spawnCount = 0;
        }
    }
}
