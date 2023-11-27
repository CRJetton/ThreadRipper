using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    [SerializeField] CharacterController characterController;
    private Vector2 currMoveInput;
    private Vector3 currMove;
    private Vector3 currSprintMove;
    [SerializeField] float moveSpeed;
    [SerializeField] float sprintMod;
    private bool isMovePressed;
    private bool isSprintPressed;

    private bool isJumpPressed;
    [SerializeField] float jumpHeight;
    [SerializeField] float gravity;

    [SerializeField] Camera playerCamera;
    [SerializeField] float sensitivity;
    [SerializeField] int lockVertMin;
    [SerializeField] int lockVertMax;
    [SerializeField] bool invertY;
    private Vector2 currLookInput;
    private Vector3 currLookRot;

    private bool isShootPressed;
    [SerializeField] float shootDist;
    [SerializeField] float rayDuration;

    //[SerializeField] Animator animator;
    //private bool isMoving;
    //private bool isSprinting;
    //private int isMovingHash;
    //private int isSprintingHash;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        //isMovingHash = Animator.StringToHash("isWalking");
        //isSprintingHash = Animator.StringToHash("isSprinting");

        //Get input
        playerInput = new PlayerInput();
        // Movement
        playerInput.PlayerControls.Move.started += OnMoveInput;
        playerInput.PlayerControls.Move.canceled += OnMoveInput;
        playerInput.PlayerControls.Move.performed += OnMoveInput;
        // Sprinting
        playerInput.PlayerControls.Sprint.started += ctx => isSprintPressed = ctx.ReadValueAsButton();
        playerInput.PlayerControls.Sprint.canceled += ctx => isSprintPressed = ctx.ReadValueAsButton();
        //Jumping
        playerInput.PlayerControls.Jump.started += ctx => isJumpPressed = ctx.ReadValueAsButton();
        playerInput.PlayerControls.Jump.canceled += ctx => isJumpPressed = ctx.ReadValueAsButton();
        // Looking
        playerInput.PlayerControls.Look.started += ctx => currLookInput = ctx.ReadValue<Vector2>();
        playerInput.PlayerControls.Look.canceled += ctx => currLookInput = ctx.ReadValue<Vector2>();
        //Shooting
        playerInput.PlayerControls.Shoot.started += OnShootInput;
        playerInput.PlayerControls.Shoot.canceled += OnShootInput;
    }

    // Update is called once per frame
    private void Update()
    {
        // Player translation
        if (isSprintPressed && characterController.isGrounded)
        {
            characterController.Move(currSprintMove * Time.deltaTime);
        }
        else characterController.Move(currMove * Time.deltaTime);

        // Player jumping and gravity
        if (characterController.isGrounded && isJumpPressed)
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

    private void OnEnable()
    {
        playerInput.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.PlayerControls.Disable();
    }

    void OnMoveInput(InputAction.CallbackContext _ctx)
    {
        currMoveInput = _ctx.ReadValue<Vector2>();
        currMove = (currMoveInput.x * transform.right * moveSpeed)
            + (currMove.y * transform.up)
            + (currMoveInput.y * transform.forward * moveSpeed);
        currSprintMove.x = currMove.x * sprintMod;
        currSprintMove.y = currMove.y;
        currSprintMove.z = currMove.z * sprintMod;
        isMovePressed = currMoveInput.x != 0 || currMoveInput.y != 0;
    }

    void OnShootInput(InputAction.CallbackContext _ctx)
    {
        isShootPressed = _ctx.ReadValueAsButton();
        if (isShootPressed)
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * shootDist,
            Color.red, rayDuration);
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f)),
                out hit, shootDist))
            {
                Debug.Log(hit.collider.gameObject.name);
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
