using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }
    public PlayerInput playerInput { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;

        playerInput = new PlayerInput();
    }

    private void OnEnable()
    {
        playerInput.PlayerControls.Enable();
        playerInput.UI.Disable();
    }

    private void OnDisable()
    {
        playerInput.PlayerControls.Disable();
        playerInput.UI.Disable();
    }

    public void SwapToPauseInput()
    {
        playerInput.PlayerControls.Disable();
        playerInput.UI.Enable();
    }

    public void SwapToPlayInput()
    {
        playerInput.UI.Disable();
        playerInput.PlayerControls.Enable();
        
    }
}
