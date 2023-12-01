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

    public PlayerController playerController;
    public PlayerBodyPositions playerBodyPositions;

    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerSpawnPos = GameObject.FindWithTag("PlayerSpawnPos");
        playerController = player.GetComponent<PlayerController>();
        playerBodyPositions = player.GetComponent<PlayerBodyPositions>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
