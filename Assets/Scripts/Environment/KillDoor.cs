using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillDoor : MonoBehaviour
{
    [SerializeField] bool startOpen;
    [SerializeField] float openTime;
    [SerializeField] float closeTime;
    [SerializeField] GameObject doorModel;

    [SerializeField] List<GameObject> enemiesToKill;

    int numToKill;
    Coroutine moveDoor;

    Vector3 closedScale;
    Vector3 closedPos;
    Vector3 openScale;
    Vector3 openPos;


    private void Start()
    {
        closedScale = doorModel.transform.localScale;
        closedPos = doorModel.transform.localPosition;
        openScale = new Vector3(doorModel.transform.localScale.x, 0, doorModel.transform.localScale.z);
        openPos = new Vector3(doorModel.transform.localPosition.x, 0, doorModel.transform.localPosition.z);

        if (startOpen)
            Open();

        foreach(GameObject enemy in enemiesToKill)
        {
            AddEnemyToKill(enemy);
        }
    }


    public void AddEnemyToKill(GameObject enemy)
    {
        numToKill++;
        enemy.GetComponent<BaseAI>().SubscribeOnDie(OnEnemyDie);
    }


    void OnEnemyDie()
    {
        numToKill--;

        if (numToKill <= 0)
            Open();
    }


    public void Open()
    {
        if (moveDoor != null)
            StopCoroutine(moveDoor);

        moveDoor = StartCoroutine(MoveDoor(openScale, openPos, openTime));
    }

    public void Close()
    {
        if (moveDoor != null)
            StopCoroutine(moveDoor);

        moveDoor = StartCoroutine(MoveDoor(closedScale, closedPos, closeTime));
    }


    IEnumerator MoveDoor(Vector3 targetScale, Vector3 targetPosition, float moveTime)
    {
        float currTime = 0f;
        Vector3 initialScale = doorModel.transform.localScale;
        Vector3 initialPosition = doorModel.transform.localPosition;

        while (currTime < openTime)
        {
            doorModel.transform.localScale = Vector3.Lerp(initialScale, targetScale, currTime / moveTime);
            doorModel.transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, currTime / moveTime);

            currTime += Time.deltaTime;
            yield return null;
        }

        moveDoor = null;
    }
}
