using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }
    [SerializeField] PlayerInput playerInput;
    [SerializeField] PlayerController player;
    [SerializeField] PlayerGroundDetector groundDetector;
    [SerializeField] PlayerVaultDetector vaultDetector;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;

        //Get input
        playerInput = new PlayerInput();
        // Movement
        playerInput.PlayerControls.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.PlayerControls.Move.canceled += ctx => moveInput = ctx.ReadValue<Vector2>();
        // Looking
        playerInput.PlayerControls.Look.started += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerInput.PlayerControls.Look.canceled += ctx => lookInput = ctx.ReadValue<Vector2>();
        // Sprinting
        playerInput.PlayerControls.Sprint.started += OnSprintInput;
        playerInput.PlayerControls.Sprint.canceled += OnSprintInput;
        //Jumping
        playerInput.PlayerControls.Jump.started += OnJumpInput;
        playerInput.PlayerControls.Jump.canceled += OnJumpInput;
    }

    private void OnEnable()
    {
        playerInput.PlayerControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.PlayerControls.Disable();
    }

    private void OnJumpInput(InputAction.CallbackContext _ctx)
    {
        if (groundDetector.GetIsPlayerGrounded() && !vaultDetector.GetCanPlayerVault()) player.SetMoveUp(player.GetJumpHeight());
        else if (vaultDetector.GetCanPlayerVault()) StartCoroutine(player.Vault(player.GetVaultTime()));
    }

    private void OnSprintInput(InputAction.CallbackContext _ctx)
    {
        if (_ctx.started && groundDetector.GetIsPlayerGrounded()) player.SetMoveSpeed(player.GetSprintSpeed());
        else player.SetMoveSpeed(player.GetDefaultSpeed());
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public Vector2 GetLookInput()
    {
        return lookInput;
    }
}
