using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/* LOG
 *
 *
 */

public class PlayerController : MonoBehaviour
{
    ////Input
    InputAction movementInput;
    InputAction sprintInput;
    InputAction lookInput;
    InputAction jumpInput;

    // Movement
    [SerializeField] private CharacterController characterController;
    private Vector3 move;
    [SerializeField] private float defaultMoveSpeed;
    private float currMoveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float lookSensitivity;

    // Jumping & gravity
    [SerializeField] private float jumpHeight;
    [SerializeField] private float gravity;

    // Vaulting
    [SerializeField] PlayerVaultDetector vaultDetector;
    [SerializeField] private float vaultTime;
    [SerializeField] float vaultForward;
    [SerializeField] float vaultUp;

    private void Start()
    {
        // Cursor handling
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Get input
        movementInput = InputManager.instance.playerInput.PlayerControls.Move;
        sprintInput = InputManager.instance.playerInput.PlayerControls.Sprint;
        sprintInput.started += OnSprintInput;
        sprintInput.canceled += OnSprintInput;
        lookInput = InputManager.instance.playerInput.PlayerControls.Look;
        jumpInput = InputManager.instance.playerInput.PlayerControls.Jump;
        jumpInput.started += OnJumpInput;

        currMoveSpeed = defaultMoveSpeed;

    }

    private void OnDisable()
    {
        sprintInput.started -= OnSprintInput;
        sprintInput.canceled -= OnSprintInput;
        jumpInput.started -= OnJumpInput;
    }

    // Update is called once per frame
    private void Update()
    {
        move = (movementInput.ReadValue<Vector2>().x * transform.right * currMoveSpeed)
            + (move.y * transform.up)
            + (movementInput.ReadValue<Vector2>().y * transform.forward * currMoveSpeed);

        characterController.Move(move * Time.deltaTime);
        Look(lookInput.ReadValue<Vector2>().x * lookSensitivity * Time.deltaTime);

        //Gravity
        if (move.y > gravity) move.y += gravity * Time.deltaTime;
    }

    public void Look(float amount)
    {
        transform.Rotate(Vector3.up, amount);
    }

    private void OnSprintInput(InputAction.CallbackContext _ctx)
    {
        if (_ctx.started) currMoveSpeed = sprintSpeed;
        else currMoveSpeed = defaultMoveSpeed;
    }

    private void OnJumpInput(InputAction.CallbackContext _ctx)
    {
        if (vaultDetector.GetCanPlayerVault()) StartCoroutine(Vault(vaultTime));
        else move.y = jumpHeight;
    }

    private IEnumerator Vault(float _vaultTime)
    {
        float time = 0;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos
            + (transform.forward * (vaultForward))
            + (transform.up * vaultUp);

        while (time < _vaultTime)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, time / _vaultTime);

            time += Time.deltaTime;
            yield return null;
        }
    }

    public float GetLookSensitivity()
    {
        return lookSensitivity;
    }
}
