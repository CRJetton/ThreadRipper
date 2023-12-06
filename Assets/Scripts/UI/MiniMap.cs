using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    Transform playerPos;
    Vector3 newPosition;
    void Start()
    {
        playerPos = GameManager.instance.player.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        newPosition = playerPos.position;
        newPosition.y = transform.position.y;
        
        transform.SetPositionAndRotation(newPosition, Quaternion.Euler(90f, GameManager.instance.player.transform.eulerAngles.y, 0f));
    }
}
