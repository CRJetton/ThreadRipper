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
    [SerializeField] private float colliderCrouchHeight;
    [SerializeField] private float colliderStandHeight;
    [SerializeField] private float colliderCrouchSizeMod;
    [SerializeField] private float cameraCrouchHeight;
    [SerializeField] private float cameraStandHeight;
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
    [SerializeField] private float vaultUpTime;
    [SerializeField] private float vaultForwardTime;
    [SerializeField] float vaultUp;
    [SerializeField] float vaultForward;

    [Header("_____Climbing_____")]
    [SerializeField] private PlayerClimbDetector climbDetector;
    [SerializeField] private float climbUpTime;
    [SerializeField] private float climbForwardTime;
    [SerializeField] private float climbUp;
    [SerializeField] private float climbForward;

    [Header("_____Animation_____")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationTransitionSpeed;
    [SerializeField] private float sprintAnimSpeed;
    private float origAnimSpeed;

    [Header("_____Audio_____")]
    [SerializeField] AudioSource playerAudio;
    [SerializeField] List<AudioClip> footStepSounds = new List<AudioClip>();
    [SerializeField] private float footStepSoundVolume;
    [SerializeField] private float footStepSoundIntervalTime;
    [SerializeField] private float sprintingFootStepSoundIntervalTime;
    private bool isMoveAudioPlaying;

    [Header("_____General_____")]
    [SerializeField] float maxHP;
    [SerializeField] private float HP;
    [SerializeField] private CameraController playerCamera;
    private GameObject spawnPos;

    [Header("-----Status Colors-----")]
    [SerializeField] Color damageColor;
    [SerializeField] Color healthColor;

    private void Awake()
    {
        // Set members
        currMoveSpeed = defaultMoveSpeed;
        HP = maxHP;
        isCrouchReady = true;
        isSlideReady = true;
        origAnimSpeed = animator.speed;
        characterController.detectCollisions = true;
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

        // Move animation & sound
        if (moveInput.ReadValue<Vector2>().magnitude != 0)
        {
            animator.SetBool("isMoving", true);

            if (groundDetector.GetIsPlayerGrounded()
                && move.normalized.magnitude > 0.3f
                && !isMoveAudioPlaying)
            {
                StartCoroutine(PlayMoveAudio());
            }
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
        animator.SetFloat("moveX", Mathf.Lerp(animator.GetFloat("moveX"), moveInput.ReadValue<Vector2>().x,
            Time.deltaTime * animationTransitionSpeed));
        animator.SetFloat("moveY", Mathf.Lerp(animator.GetFloat("moveY"), moveInput.ReadValue<Vector2>().y,
            Time.deltaTime * animationTransitionSpeed));

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
            isSprinting = true;
            animator.speed = sprintAnimSpeed;
            currMoveSpeed = sprintSpeed;
        }
        else
        {
            isSprinting = false;
            animator.speed = origAnimSpeed;
            currMoveSpeed = defaultMoveSpeed;
        }
    }

    private void OnJumpInput(InputAction.CallbackContext _ctx)
    {

        if (_ctx.started && vaultDetector.GetCanPlayerVault())
        {
            StartCoroutine(ClimbOver(vaultUpTime, vaultForwardTime, vaultUp, vaultForward));
            animator.SetTrigger("isJumping");
        }
        else if (_ctx.started && !groundDetector.GetIsPlayerGrounded() && climbDetector.GetCanPlayerClimb())
        {
            StartCoroutine(ClimbOver(climbUpTime, climbForwardTime, climbUp, climbForward));
            animator.SetTrigger("isJumping");
        }
        else if (_ctx.started && groundDetector.GetIsPlayerGrounded())
        {
            move.y = jumpHeight;
            animator.SetTrigger("isJumping");
        }
    }

    private IEnumerator ClimbOver(float _upTime, float _forwardTime, float _targetUp, float _targetForward)
    {
        float currTime = 0;
        Vector3 startPos = transform.localPosition;
        Vector3 targetPosUp = startPos + transform.up * _targetUp;

        while (currTime < _upTime)
        {
            Vector3 moveUp = Vector3.Lerp(startPos, targetPosUp, currTime / _forwardTime);
            transform.localPosition = moveUp;
            currTime += Time.deltaTime;
            yield return null;
        }

        currTime = 0;
        startPos = transform.localPosition;
        Vector3 targetPositionForward = startPos + transform.forward * _targetForward;


        while (currTime < _forwardTime)
        {
            Vector3 moveForward = Vector3.Lerp(startPos, targetPositionForward, currTime / _forwardTime);
            transform.localPosition = moveForward;
            currTime += Time.deltaTime;
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
            animator.SetBool("isCrouching", true);
            Crouch();
        }
        else if (_ctx.started && isSprinting && isSlideReady)
        {
            animator.SetBool("isCrouching", true);
            Crouch();
            StartCoroutine(SprintSlide(sprintSlideTime, sprintSlideSpeed));
            StartCoroutine(SlideCooldown());
        }
        else if (_ctx.canceled && !isSliding && !isStanding)
        {
            animator.SetBool("isCrouching", false);
            Stand();
        }
    }

    private void Crouch()
    {
        StartCoroutine(playerCamera.Crouch(cameraCrouchHeight, crouchTime));
        StartCoroutine(CrouchCooldown());
        characterController.center = new Vector3(0, colliderCrouchHeight, 0);
        characterController.height /= colliderCrouchSizeMod;
        GameManager.instance.playerBodyPositions.Crouch();
        currMoveSpeed = crouchMoveSpeed;
        isCrouching = true;
        isStanding = false;
    }

    private void Stand()
    {
        StartCoroutine(playerCamera.Crouch(cameraStandHeight, crouchTime));
        StartCoroutine(CrouchCooldown());
        characterController.center = new Vector3(0, colliderStandHeight, 0);
        characterController.height *= colliderCrouchSizeMod;
        GameManager.instance.playerBodyPositions.Crouch();
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
        animator.SetTrigger("isSliding");
        isSliding = true;
        float time = 0;
        
        while (time < _sprintSlideTime && isSliding)
        {
            currMoveSpeed = _sprintSlideSpeed;
            time += Time.deltaTime;
            yield return null;
        }

        animator.SetBool("isCrouching", false);
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
        StartCoroutine(HUDManager.instance.FlashColor(0,1,damageColor));
        HP -= _damage;
        HUDManager.instance.playerHPBar.fillAmount = HP / maxHP;
        if (HP <= 0) UIManager.instance.YouLose();
    }

    public void AddHP(float _health)
    {
        StartCoroutine(HUDManager.instance.FlashColor(0,2,healthColor));
        HP += _health;
        if (HP > maxHP)
        {
            HP = maxHP;
        }
        HUDManager.instance.playerHPBar.fillAmount = HP / maxHP;
    }

    private IEnumerator PlayMoveAudio()
    {
        isMoveAudioPlaying = true;

        int i = Random.Range(0, footStepSounds.Count - 1);
        playerAudio.PlayOneShot(footStepSounds[i], footStepSoundVolume);

        if (!isSprinting)
        {
            yield return new WaitForSeconds(footStepSoundIntervalTime);
        }
        else
        {
            yield return new WaitForSeconds(sprintingFootStepSoundIntervalTime);
        }

        isMoveAudioPlaying = false;
    }
}
