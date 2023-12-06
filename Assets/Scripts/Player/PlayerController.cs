using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 LOG

TO DO
- Remember for final product: Optimize gameobject animations
- Animate move
- Fix climbing so you go up and then forward
- Add kinematic Rigidbody
- Make vault detector work like ground detector with list

DONE

 */

public class PlayerController : MonoBehaviour, IDamageable
{
    // Input
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

    [Header("_____Crouching_____")]
    [SerializeField] private float cameraCrouchAmount;
    [SerializeField] private float colliderCrouchAmount;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float standHeight;
    [SerializeField] private float crouchTime;
    [SerializeField] private float crouchCooldown;
    private bool isCrouching;
    private bool isStanding;
    private bool isCrouchReady;

    [Header("_____Sliding_____")]
    [SerializeField] private float sprintSlideTime;
    [SerializeField] private float sprintSlideSpeed;
    [SerializeField] private float slideCooldown;
    private bool isSliding;
    private bool isSlideReady;

    [Header("_____Jumping/Gravity_____")]
    [SerializeField] private PlayerGroundDetector groundDetector;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float gravity;

    [Header("_____Vaulting_____")]
    [SerializeField] private PlayerVaultDetector vaultDetector;
    [SerializeField] private float vaultTime;
    [SerializeField] float vaultForward;
    [SerializeField] float vaultUp;

    [Header("_____Climbing_____")]
    [SerializeField] private PlayerClimbDetector climbDetector;
    [SerializeField] private float climbTime;
    [SerializeField] private float climbForward;
    [SerializeField] private float climbUp;

    [Header("_____Animation_____")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationTransitionSpeed;

    [Header("_____General_____")]
    [SerializeField] float maxHP;
    [SerializeField] private CameraController playerCamera;
    private float HP;
    private GameObject spawnPos;

    private void Awake()
    {
        // Set members
        currMoveSpeed = defaultMoveSpeed;
        HP = maxHP;
        isCrouchReady = true;
        isSlideReady = true;
    }

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
        Look(lookInput.ReadValue<Vector2>().x 
            * SettingsManager.instance.lookSensitivity * Time.deltaTime);

        // Move animation
        animator.SetFloat("X", Mathf.Lerp(animator.GetFloat("X"), moveInput.ReadValue<Vector2>().x,
            Time.deltaTime * animationTransitionSpeed));
        animator.SetFloat("Y", Mathf.Lerp(animator.GetFloat("Y"), moveInput.ReadValue<Vector2>().y,
            Time.deltaTime * animationTransitionSpeed)); ;

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
            StartCoroutine(ClimbOver(vaultTime, vaultForward, vaultUp));
        }
        else if (_ctx.started && !groundDetector.GetIsPlayerGrounded() && climbDetector.GetCanPlayerClimb())
        {
            StartCoroutine(ClimbOver(climbTime, climbForward, climbUp));
        }
        else if (_ctx.started && groundDetector.GetIsPlayerGrounded())
        {
            move.y = jumpHeight;
        }
    }

    private IEnumerator ClimbOver(float _time, float _forward, float _up)
    {
        float time = 0;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos
            + (transform.forward * _forward)
            + (transform.up * _up);

        while (time < _time)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, time / _time);

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
        else if (_ctx.started && !isCrouchReady)
        {
            return;
        }
        else if (_ctx.started && !isSprinting)
        {
            Crouch();
        }
        else if (_ctx.started && isSprinting && isSlideReady)
        {
            Crouch();
            StartCoroutine(SprintSlide(sprintSlideTime, sprintSlideSpeed));
            StartCoroutine(SlideCooldown());
        }
        else if (_ctx.canceled && !isSliding && !isStanding)
        {
            Stand();
        }
    }

    private void Crouch()
    {
        StartCoroutine(playerCamera.Crouch(crouchHeight, crouchTime));
        StartCoroutine(CrouchCooldown());
        characterController.height /= colliderCrouchAmount;
        currMoveSpeed = crouchMoveSpeed;
        isCrouching = true;
        isStanding = false;
    }

    private void Stand()
    {
        StartCoroutine(playerCamera.Crouch(standHeight, crouchTime));
        StartCoroutine(CrouchCooldown());
        characterController.height *= colliderCrouchAmount;
        currMoveSpeed = defaultMoveSpeed;
        isCrouching = false;
        isStanding = true;
    }

    private IEnumerator CrouchCooldown()
    {
        isCrouchReady = false;
        yield return new WaitForSeconds(crouchCooldown);
        isCrouchReady = true;
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

    private IEnumerator SlideCooldown()
    {
        isSlideReady = false;
        yield return new WaitForSeconds(slideCooldown);
        isSlideReady = true;
    }

    public void OnRespawn()
    {
        characterController.enabled = false;
        transform.position = spawnPos.transform.position;
        characterController.enabled = true;
        HP = maxHP;
        HUDManager.instance.playerHPBar.fillAmount = HP;
    }

    public void TakeDamage(float _damage)
    {
        HUDManager.instance.FlashDamage();
        HP -= _damage;
        HUDManager.instance.playerHPBar.fillAmount = HP / maxHP;
        if (HP <= 0) UIManager.instance.YouLose();
    }

    public void AddHP(float _health)
    {
        //HUDManager.instance.FlashHealth();
        HP += _health;
        HUDManager.instance.playerHPBar.fillAmount = HP / maxHP;
    }
}
