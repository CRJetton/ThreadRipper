using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 LOG

TO DO
- Mark all new work as new incase bugs in build!!!!!!!!!!!!!!!!!!!!!
- Queue jump for end of sprint slide

DONE


 */

public class PlayerController : MonoBehaviour, IDamageable
{
    //Input
    InputAction moveInput;
    InputAction lookInput;
    InputAction sprintInput;
    InputAction jumpInput;
    InputAction crouchInput;

    [Header("_____Movement_____")]
    [SerializeField] private CharacterController characterController;
    private Vector3 move;
    [SerializeField] private float defaultMoveSpeed;
    private float currMoveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float crouchMoveSpeed;
    private bool isSprinting;
    [SerializeField] private float cameraCrouchAmount;
    [SerializeField] private float colliderCrouchAmount;
    private bool isCrouching;
    private bool isStanding;
    [SerializeField] private float sprintSlideTime;
    [SerializeField] private float sprintSlideSpeed;
    private bool isSliding;


    [Header("_____Jumping/Gravity_____")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float gravity;
    private bool isJumping;

    [Header("_____Vaulting_____")]
    [SerializeField] PlayerVaultDetector vaultDetector;
    [SerializeField] PlayerGroundDetector groundDetector;
    [SerializeField] private float vaultTime;
    [SerializeField] float vaultForward;
    [SerializeField] float vaultUp;

    [Header("_____General_____")]
    [SerializeField] float maxHP;
    [SerializeField] CameraController playerCamera;
    private float HP;
    private GameObject spawnPos;


    private void Start()
    {
        // Cursor handling
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Get input
        moveInput = InputManager.instance.playerInput.PlayerControls.Move;
        lookInput = InputManager.instance.playerInput.PlayerControls.Look;
        sprintInput = InputManager.instance.playerInput.PlayerControls.Sprint;
        sprintInput.started += OnSprintInput;
        sprintInput.canceled += OnSprintInput;
        jumpInput = InputManager.instance.playerInput.PlayerControls.Jump;
        jumpInput.started += OnJumpInput;
        crouchInput = InputManager.instance.playerInput.PlayerControls.Crouch;
        crouchInput.started += OnCrouchInput;
        crouchInput.canceled += OnCrouchInput;

        // Set members
        currMoveSpeed = defaultMoveSpeed;
        HP = maxHP;
        HUDManager.instance.playerHPBar.fillAmount = HP;
        spawnPos = GameObject.FindWithTag("PlayerSpawnPos");
    }

    // Update is called once per frame
    private void Update()
    {
        // Move and look
        move = (moveInput.ReadValue<Vector2>().x * transform.right * currMoveSpeed)
            + (move.y * transform.up)
            + (moveInput.ReadValue<Vector2>().y * transform.forward * currMoveSpeed);
        characterController.Move(move * Time.deltaTime);
        Look(lookInput.ReadValue<Vector2>().x * SettingsManager.instance.lookSensitivity * Time.deltaTime);

        //Gravity
        if (move.y > gravity)
        {
            move.y += gravity * Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        // Sever ties with input callback functions upon object's destruction
        sprintInput.started -= OnSprintInput;
        sprintInput.canceled -= OnSprintInput;
        jumpInput.started -= OnJumpInput;
        crouchInput.started -= OnCrouchInput;
        crouchInput.canceled -= OnCrouchInput;
    }

    public void Look(float amount)
    {
        // Made public function to allow PlayerCombat access for recoil effect.
        transform.Rotate(Vector3.up, amount);
    }

    private void OnSprintInput(InputAction.CallbackContext _ctx)
    {
        if (isCrouching && !isSliding)
        {
            return;
        }
        else if (_ctx.started && moveInput.ReadValue<Vector2>().y >= 0 && groundDetector.GetIsPlayerGrounded())
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
        if (_ctx.started && vaultDetector.GetCanPlayerVault())
        {
            StartCoroutine(Vault(vaultTime));
        }
        else if (_ctx.started && groundDetector.GetIsPlayerGrounded())
        {
            move.y = jumpHeight;
        }
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

    private void OnCrouchInput(InputAction.CallbackContext _ctx)
    {
        if (isSliding)
        {
            return;
        }
        else if (_ctx.started && !isSprinting)
        {
            Crouch();
        }
        else if (_ctx.started && isSprinting)
        {
            Crouch();
            StartCoroutine(SprintSlide(sprintSlideTime, sprintSlideSpeed));
        }
        else if (_ctx.canceled && !isSliding && !isStanding)
        {
            Stand();
        }
    }

    private void Crouch()
    {
        Vector3 crouch = new Vector3(0, -cameraCrouchAmount, 0);
        playerCamera.Translate(crouch);
        characterController.height /= colliderCrouchAmount;
        currMoveSpeed = crouchMoveSpeed;
        isCrouching = true;
        isStanding = false;
    }

    private void Stand()
    {
        Vector3 stand = new Vector3(0, cameraCrouchAmount, 0);
        playerCamera.Translate(stand);
        characterController.height *= colliderCrouchAmount;
        currMoveSpeed = defaultMoveSpeed;
        isCrouching = false;
        isStanding = true;
    }

    private IEnumerator SprintSlide(float _sprintSlideTime, float _sprintSlideSpeed)
    {
        isSliding = true;
        float time = 0;
        
        while (time < _sprintSlideTime && isSliding)
        {
            currMoveSpeed = _sprintSlideSpeed;
            time += Time.deltaTime;
            yield return null;
        }

        Stand();
        isSliding = false;

        if (isSprinting)
        {
            currMoveSpeed = sprintSpeed;
        }
        else
        {
            currMoveSpeed = defaultMoveSpeed;
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

    public void TakeDamage(int _damage)
    {
        HUDManager.instance.FlashDamage();
        HP -= _damage;
        HUDManager.instance.playerHPBar.fillAmount = HP / maxHP;
        if (HP <= 0) UIManager.instance.YouLose();
    }
}
