using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/* LOG
 *
 * Player can now only sprint in forward directions.
 * 
 */

public class PlayerController : MonoBehaviour, IDamageable
{
    // General
    [SerializeField] float maxHP;
    private float HP;
    [SerializeField] GameObject spawnPos;
    [SerializeField] CameraController playerCamera;

    ////Input
    InputAction moveInput;
    InputAction lookInput;
    InputAction sprintInput;
    InputAction jumpInput;
    InputAction crouchInput;

    // Movement
    [SerializeField] private CharacterController characterController;
    private Vector3 move;
    [SerializeField] private float defaultMoveSpeed;
    private float currMoveSpeed;
    private bool isSprinting;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private float cameraCrouchAmount;
    [SerializeField] private float colliderCrouchAmount;

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
        moveInput = InputManager.instance.playerInput.PlayerControls.Move;
        moveInput.started += OnMoveInput;
        lookInput = InputManager.instance.playerInput.PlayerControls.Look;
        sprintInput = InputManager.instance.playerInput.PlayerControls.Sprint;
        sprintInput.started += OnSprintInput;
        sprintInput.canceled += OnSprintInput;
        jumpInput = InputManager.instance.playerInput.PlayerControls.Jump;
        jumpInput.started += OnJumpInput;
        crouchInput = InputManager.instance.playerInput.PlayerControls.Crouch;
        crouchInput.started += OnCrouchInput;
        crouchInput.canceled += OnCrouchInput;

        currMoveSpeed = defaultMoveSpeed;
        HP = maxHP;
        HUDManager.instance.playerHPBar.fillAmount = HP;
    }

    // Update is called once per frame
    private void Update()
    {
        move = (moveInput.ReadValue<Vector2>().x * transform.right * currMoveSpeed)
            + (move.y * transform.up)
            + (moveInput.ReadValue<Vector2>().y * transform.forward * currMoveSpeed);

        characterController.Move(move * Time.deltaTime);
        Look(lookInput.ReadValue<Vector2>().x * lookSensitivity * Time.deltaTime);

        //Gravity
        if (move.y > gravity) move.y += gravity * Time.deltaTime;
    }

    private void OnDisable()
    {
        moveInput.started -= OnMoveInput;
        sprintInput.started -= OnSprintInput;
        sprintInput.canceled -= OnSprintInput;
        jumpInput.started -= OnJumpInput;
        crouchInput.started -= OnCrouchInput;
        crouchInput.canceled -= OnCrouchInput;
    }

    public void Look(float amount)
    {
        transform.Rotate(Vector3.up, amount);
    }

    private void OnMoveInput(InputAction.CallbackContext _ctx)
    {
        if (isSprinting && moveInput.ReadValue<Vector2>().y >= 0) currMoveSpeed = sprintSpeed;
        else currMoveSpeed = defaultMoveSpeed;
    }

    private void OnSprintInput(InputAction.CallbackContext _ctx)
    {
        if (_ctx.started && moveInput.ReadValue<Vector2>().y >= 0)
        {
            currMoveSpeed = sprintSpeed;
            isSprinting = true;
        }
        else
        {
            currMoveSpeed = defaultMoveSpeed;
            isSprinting = false;
        }
    }

    private void OnJumpInput(InputAction.CallbackContext _ctx)
    {
        if (vaultDetector.GetCanPlayerVault()) StartCoroutine(Vault(vaultTime));
        else move.y = jumpHeight;
    }

    private void OnCrouchInput(InputAction.CallbackContext _ctx)
    {

        if (_ctx.started)
        {
            Vector3 crouch = new Vector3(0, -cameraCrouchAmount, 0);
            playerCamera.Translate(crouch);
            //characterController.radius /= 2;
            characterController.height /= colliderCrouchAmount;
            Debug.Log(playerCamera.transform.position);
        }
        else
        {
            Vector3 stand = new Vector3(0, cameraCrouchAmount, 0);
            playerCamera.Translate(stand);
            //characterController.radius *= 2;
            characterController.height *= colliderCrouchAmount;
            Debug.Log(playerCamera.transform.position);
        }
    }

    public void OnRespawn()
    {
        characterController.enabled = false;
        transform.position = spawnPos.transform.position;
        characterController.enabled = true;
        HP = maxHP;
        HUDManager.instance.playerHPBar.fillAmount = HP;
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

    public void TakeDamage(int _damage)
    {
        HP -= _damage;
        HUDManager.instance.playerHPBar.fillAmount = HP / maxHP;
        if (HP <= 0) UIManager.instance.YouLose();
    }

    public float GetLookSensitivity()
    {
        return lookSensitivity;
    }
}
