using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject menuActive;
    public GameObject menuPause;
    public GameObject menuWin;
    public GameObject menuLose;
    public InputAction playerPauseInput;
    public InputAction UIPauseInput;

    public bool isPaused;
    float originalTimeScale;


    void Awake()
    {
        instance = this;
        originalTimeScale = Time.timeScale;

    }

    void Start()
    {
        playerPauseInput = InputManager.instance.playerInput.PlayerControls.Pause;
        UIPauseInput = InputManager.instance.playerInput.UI.Pause;

        playerPauseInput.started += PauseMenu;
        UIPauseInput.started += UnpauseMenu;
    }

    private void OnDisable()
    {
        playerPauseInput.started -= PauseMenu;
        UIPauseInput.started -= UnpauseMenu;
    }

    public void StatePaused()
    {
        isPaused = !isPaused;
        InputManager.instance.SwapToPauseInput();
        HideHUD();
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void StateUnpaused()
    {
        InputManager.instance.SwapToPlayInput();
        isPaused = !isPaused;
        ShowHUD();
        Time.timeScale = originalTimeScale;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void YouLose()
    {
        StatePaused();
        HideHUD();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void HideHUD()
    {
        HUDManager.instance.playerHPBarFrame.SetActive(false);
        HUDManager.instance.ammoCount.SetActive(false);
        HUDManager.instance.enemiesBackground.SetActive(false);
        HUDManager.instance.reticleController.gameObject.SetActive(false);
        HUDManager.instance.damageScreen.SetActive(false);
        HUDManager.instance.minimap.SetActive(false);
    }

    public void ShowHUD()
    {
        HUDManager.instance.playerHPBarFrame.SetActive(true);
        HUDManager.instance.ammoCount.SetActive(true);
        HUDManager.instance.enemiesBackground.SetActive(true);
        HUDManager.instance.reticleController.gameObject.SetActive(true);
        HUDManager.instance.damageScreen.SetActive(true);
        HUDManager.instance.minimap.SetActive(true);
    }

    private void PauseMenu(InputAction.CallbackContext context)
    {
        StatePaused();
        menuActive = menuPause;
        menuActive.SetActive(isPaused);
    }

    private void UnpauseMenu(InputAction.CallbackContext context)
    {
        StateUnpaused();
        menuActive = menuPause;
        menuActive.SetActive(isPaused);
    }
}
