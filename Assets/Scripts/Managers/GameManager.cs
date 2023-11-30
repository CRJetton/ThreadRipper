using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{    
    public static GameManager instance;
    public GameObject playerSpawnPos;
    public GameObject player;

    public PlayerController playerScript;

    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
        playerSpawnPos = GameObject.FindWithTag("PlayerSpawnPos");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
