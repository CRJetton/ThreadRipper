using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{    
    public static GameManager instance;
    public GameObject playerSpawnPos;
    public GameObject player;
    public GameObject playerPositions;

    public PlayerController playerScript;
    public PlayerPositions positionScript;

    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerPositions = GameObject.FindWithTag("PlayerPositions");
        playerSpawnPos = GameObject.FindWithTag("PlayerSpawnPos");
        playerScript = player.GetComponent<PlayerController>();
        positionScript = playerPositions.GetComponent<PlayerPositions>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
