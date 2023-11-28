using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 * Change how we detected the player is grounded
 * Seperate camea logic into its own class
 */

public class PlayerController : MonoBehaviour
{
    // Movement & Sprinting
    private PlayerInput playerInput;
    [SerializeField] CharacterController characterController;
    private Vector2 currMoveInput;
    private Vector3 currMove;
    private Vector3 currSprintMove;
    [SerializeField] float moveSpeed;
    [SerializeField] float sprintMod;
    private bool isMovePressed;
    private bool isSprintPressed;

    // Jumping & gravity
    private bool isGrounded;
    private bool isJumpPressed;
    [SerializeField] float jumpHeight;
    [SerializeField] float gravity;

    // Vaulting
    private bool canVault;
    [SerializeField] float vaultTime;
    [SerializeField] float vaultBuffer;

    // Camera control
    [SerializeField] Camera playerCamera;
    [SerializeField] float sensitivity;
    [SerializeField] int lockVertMin;
    [SerializeField] int lockVertMax;
    [SerializeField] bool invertY;
    private Vector2 currLookInput;
    private Vector3 currLookRot;

    // Shooting
    private bool isShootPressed;
    [SerializeField] float shootDist;
    [SerializeField] float shootRayEndurance;

    //[SerializeField] Animator animator;
    //private bool isMoving;
    //private bool isSprinting;
    //private int isMovingHash;
    //private int isSprintingHash;

    private void Awake()
    {
        //Get input
        playerInput = new PlayerInput();
        // Movement
        playerInput.PlayerControls.Move.performed += ctx => currMoveInput = ctx.ReadValue<Vector2>();
        //playerInput.PlayerControls.Move.started += ctx => currMoveInput = ctx.ReadValue<Vector2>();
        playerInput.PlayerControls.Move.canceled += ctx => currMoveInput = ctx.ReadValue<Vector2>();
        // Looking
        playerInput.PlayerControls.Look.started += ctx => currLookInput = ctx.ReadValue<Vector2>();
        playerInput.PlayerControls.Look.canceled += ctx => currLookInput = ctx.ReadValue<Vector2>();
        // Sprinting
        playerInput.PlayerControls.Sprint.started += ctx => isSprintPressed = ctx.ReadValueAsButton();
        playerInput.PlayerControls.Sprint.canceled += ctx => isSprintPressed = ctx.ReadValueAsButton();
        //Jumping
        playerInput.PlayerControls.Jump.started += ctx => isJumpPressed = ctx.ReadValueAsButton();
        playerInput.PlayerControls.Jump.canceled += ctx => isJumpPressed = ctx.ReadValueAsButton();
        //Shooting
        playerInput.PlayerControls.Shoot.started += OnShootInput;
        playerInput.PlayerControls.Shoot.canceled += OnShootInput;
    }

    private void OnEnable()
    {
        playerInput.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.PlayerControls.Disable();
    }

    private void Start()
    {
        // Cursor handling
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //isMovingHash = Animator.StringToHash("isWalking");
        //isSprintingHash = Animator.StringToHash("isSprinting");
    }

    // Update is called once per frame
    private void Update()
    {
        // Movement vector updates
        currMove = (currMoveInput.x * transform.right * moveSpeed)
            + (currMove.y * transform.up)
            + (currMoveInput.y * transform.forward * moveSpeed);
        currSprintMove.x = currMove.x * sprintMod;
        currSprintMove.y = currMove.y;
        currSprintMove.z = currMove.z * sprintMod;
        isMovePressed = currMoveInput.x != 0 || currMoveInput.y != 0;

        // Player translation
        if (isSprintPressed && characterController.isGrounded)
        {
            characterController.Move(currSprintMove * Time.deltaTime);
        }
        else characterController.Move(currMove * Time.deltaTime);

        // Player jumping and gravity
        if (characterController.isGrounded && isJumpPressed && !canVault)
        {
            currMove.y = jumpHeight;
        }
        else if (currMove.y > gravity)
        {
            currMove.y += gravity * Time.deltaTime;
        }
        else currMove.y = gravity;

        // Player rotation
        transform.Rotate(Vector3.up * currLookInput.x * sensitivity * Time.deltaTime);

        //HandleAnimation();
    }

    private void LateUpdate()
    {
        // Camera rotation
        currLookRot.x -= currLookInput.y * sensitivity * Time.deltaTime;
        currLookRot.x = Mathf.Clamp(currLookRot.x, lockVertMin, lockVertMax);
        currLookRot.y = transform.rotation.eulerAngles.y;
        playerCamera.transform.rotation = Quaternion.Euler(currLookRot);
    }

    private void OnCollisionEnter(Collision _other)
    {
        if (_other.gameObject.CompareTag("Ground")) isGrounded = true;
        Debug.Log(isGrounded);
    }

    private void OnCollisionExit(Collision _other)
    {
        if (_other.gameObject.CompareTag("Ground")) isGrounded = false;
        Debug.Log(isGrounded);
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.layer == LayerMask.NameToLayer("Vault")) canVault = true;
    }

    private void OnTriggerStay(Collider _other)
    {
        if (canVault && isJumpPressed)
        {
            StartCoroutine(Vault(_other.gameObject, vaultTime));
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.gameObject.layer == LayerMask.NameToLayer("Vault")) canVault = false;
        Debug.Log(characterController.isGrounded);
    }

    void OnMoveInput(InputAction.CallbackContext _ctx)
    {
        //currMoveInput = _ctx.ReadValue<Vector2>();
        //currLookInput = _ctx.ReadValue<Vector2>();
        //currMove = (currMoveInput.x * transform.right * moveSpeed)
        //    + (currMove.y * transform.up)
        //    + (currMoveInput.y * transform.forward * moveSpeed);
        //currSprintMove.x = currMove.x * sprintMod;
        //currSprintMove.y = currMove.y;
        //currSprintMove.z = currMove.z * sprintMod;
        //isMovePressed = currMoveInput.x != 0 || currMoveInput.y != 0;
    }

    IEnumerator Vault(GameObject _vaultObstacle, float _vaultTime)
    {
        float time = 0;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos
            + (transform.forward * (_vaultObstacle.transform.localScale.z + vaultBuffer))
            + (transform.up * _vaultObstacle.transform.localScale.y);

        while (time < _vaultTime)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, time / _vaultTime);

            time += Time.deltaTime;
            yield return null;
        }

        characterController.Move(currMove * Time.deltaTime);
        canVault = true;
    }

    void OnShootInput(InputAction.CallbackContext _ctx)
    {
        isShootPressed = _ctx.ReadValueAsButton();
        if (isShootPressed)
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * shootDist,
            Color.red, shootRayEndurance);
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f)),
                out hit, shootDist))
            {
                //Debug.Log(hit.collider.gameObject.name);
            }
        }
    }

    //void HandleAnimation()
    //{
    //    isMoving = animator.GetBool(isMovingHash);
    //    isSprinting = animator.GetBool(isSprintingHash);

    //    if (isMovePressed && !isMoving) animator.SetBool(isMovingHash, true);
    //    else if (!isMovePressed && isMoving) animator.SetBool(isMovingHash, false);
    //    else if (isMovePressed && isSprintPressed && !isSprinting)
    //        animator.SetBool(isSprintingHash, true);
    //    else if (isMovePressed && !isSprintPressed && isSprinting)
    //        animator.SetBool(isSprintingHash, false);
    //}

}
