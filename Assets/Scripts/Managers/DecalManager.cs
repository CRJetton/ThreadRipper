using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalManager : MonoBehaviour
{
    public static DecalManager instance;

    [Header("Bullet Holes")]
    [SerializeField, Range(0, 100)] int maxBulletHoles;
    [SerializeField, Range(0f, 1f)] float bulletHoleSpawnOffset;
    [SerializeField] GameObject bulletHolePrefab;

    int bulletHoleIndex;
    List<GameObject> bulletHoles = new List<GameObject>();


    private void Awake()
    {
        instance = this;
    }


    public void CreateBulletHole(Vector3 pos, Vector3 dir)
    {
        if (bulletHoleIndex < maxBulletHoles)
            bulletHoleIndex++;
        else
            bulletHoleIndex = 0;


        Vector3 offsetPos = pos;
        offsetPos.x -= dir.x * bulletHoleSpawnOffset;
        offsetPos.y -= dir.y * bulletHoleSpawnOffset;
        offsetPos.z -= dir.z * bulletHoleSpawnOffset;

        if (bulletHoles.Count < maxBulletHoles)
            InstantiateBulletHole(offsetPos, dir);
        else
            MoveBulletHole(offsetPos, dir);
    }

    void InstantiateBulletHole(Vector3 pos, Vector3 dir)
    {
        GameObject bulletHole = Instantiate(bulletHolePrefab, pos, Quaternion.identity);
        Quaternion rot = Quaternion.LookRotation(dir);

        bulletHole.transform.rotation = rot;
        bulletHole.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

        bulletHoles.Add(bulletHole);
    }

    void MoveBulletHole(Vector3 pos, Vector3 dir)
    {
        Quaternion rot = Quaternion.LookRotation(dir);

        bulletHoles[bulletHoleIndex].transform.position = pos;
        bulletHoles[bulletHoleIndex].transform.rotation = rot;
        bulletHoles[bulletHoleIndex].transform.Rotate(Vector3.forward, Random.Range(0f, 360f));
    }
}
