using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Teleport : MonoBehaviour
{
    [SerializeField] Transform Destination;
    [SerializeField] GameObject player;
    KeyCode teleKey;

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
        player.transform.position = Destination.transform.position;
        }
    }
}
