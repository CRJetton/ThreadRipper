using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{   
    public enum GameStates
    {
        play = 0,
        pauseMenu = 1,
        winMenu = 2,
        loseMenu = 3,
        settingsMenu = 4,
        mainMenu = 5,
        



    };

    public static GameManager instance;

    [Header("-----Player-----")]
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

}
