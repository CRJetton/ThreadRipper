using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("-----Menus-----")]
    public GameObject menuActive;
    public GameObject menuPause;
    public GameObject menuWin;
    public GameObject menuLose;
    public GameObject popupMenu;
    [SerializeField] TMP_Text popupText;

    [Header("-----Inputs-----")]
    public InputAction playerPauseInput;
    public InputAction UIPauseInput;

    [Header("-----General-----")]
    public bool isPaused;
    float originalTimeScale;
    private GameManager.GameStates currState;


    void Awake()
    {
        instance = this;
        originalTimeScale = Time.timeScale;
        currState = GameManager.GameStates.play;

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
        currState = GameManager.GameStates.play;
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
        currState = GameManager.GameStates.loseMenu;
        StatePaused();
        HideHUD();
        InputManager.instance.playerInput.UI.Pause.Disable();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public IEnumerator YouWin()
    {
        yield return new WaitForSeconds(3);
        currState = GameManager.GameStates.winMenu;
        StatePaused();
        menuActive = menuWin;
        menuActive.SetActive(true);

    }

    public void HideHUD()
    {
        HUDManager.instance.playerHPBarFrame.SetActive(false);
        HUDManager.instance.ammoCount.SetActive(false);
        HUDManager.instance.enemiesBackground.SetActive(false);
        HUDManager.instance.reticleController.gameObject.SetActive(false);
        HUDManager.instance.statusObject.SetActive(false);
        HUDManager.instance.minimap.SetActive(false);
    }

    public void ShowHUD()
    {
        HUDManager.instance.playerHPBarFrame.SetActive(true);
        HUDManager.instance.ammoCount.SetActive(true);
        HUDManager.instance.enemiesBackground.SetActive(true);
        HUDManager.instance.reticleController.gameObject.SetActive(true);
        HUDManager.instance.statusObject.SetActive(true);
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
        if(currState == GameManager.GameStates.loseMenu || 
           currState == GameManager.GameStates.winMenu)
        {
            return;
        }
        else
        {
            StateUnpaused();
            menuActive = menuPause;
            menuActive.SetActive(isPaused);
        }
        
    }

    void CreatePopup(Transform position, string itemName)
    {
        popupMenu.transform.position = position.position;
        popupText.text = itemName;
        popupMenu.SetActive(true);
        
    }
}
