using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportMaker : MonoBehaviour
{
    public GameObject telePrefab;
    public GameObject teleObject;
    [SerializeField] GameObject player;
    [SerializeField] Transform Destination;

    [Header("Teleport Stats")]
    [SerializeField] float teleCooldownTime;

    private float nextTeleTime;

    void Start()
    {
        
    }
    void Update()
    {
       
        if (nextTeleTime >= teleCooldownTime)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // Makes the teleport object 
                teleObject = Instantiate(telePrefab);
                teleObject.transform.position = player.transform.position + player.transform.forward;
                teleObject.transform.forward = player.transform.forward;
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                nextTeleTime = Time.deltaTime + teleCooldownTime;
                //teleports the player to that object
                player.transform.position = teleObject.transform.position;
                Destroy(teleObject);
            }
        }
    }
}
