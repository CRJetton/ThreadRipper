using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TeleportMaker : MonoBehaviour
{
    InputAction teleInput;
    public GameObject telePrefab;
    public GameObject teleObject;
    [SerializeField] GameObject player;

    [Header("Teleport Stats")]
    [SerializeField] float teleCooldownTime;

    private float nextTeleTime;

    void Start()
    {
        teleInput = InputManager.instance.playerInput.PlayerControls.Teleport;
        teleInput.started += OnTeleInputDown;
        teleInput.canceled += OnTeleInputUp;



    }
    void Update()
    {
       

    }
    IEnumerator teleCooldown()
    {
        yield return new WaitForSeconds(nextTeleTime);
    }

    public void OnTeleInputDown(InputAction.CallbackContext _ctx)
    {
        if (_ctx.started)
        {
            //Makes the teleport object 
            teleObject = Instantiate(telePrefab);
            //Moves the teleport position
            teleObject.transform.position = player.transform.position + player.transform.forward;
            teleObject.transform.forward = player.transform.forward;
        }
    }
    public void OnTeleInputUp(InputAction.CallbackContext _ctx)
    {
        if (_ctx.canceled)
        {
            //teleports the player to that object
            player.transform.position = teleObject.transform.position;
            //Starts Cooldown
            nextTeleTime = teleCooldownTime;
            StartCoroutine(teleCooldown());
            //Destroys teleObject
            Destroy(teleObject);
        }
    }
}
