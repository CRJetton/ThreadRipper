using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("-----Menus-----")]
    public GameObject menuActive;
    public GameObject menuPause;
    public GameObject menuWin;
    public GameObject menuLose;
    public GameObject menuSettings;
    public GameObject popupMenu;
    [SerializeField] TMP_Text popupText;

    [Header("-----Inputs-----")]
    public InputAction playerPauseInput;
    public InputAction UIPauseInput;

    [Header("-----General-----")]
    public bool isPaused;
    float originalTimeScale;
    public GameManager.GameStates currState;
    [SerializeField] Texture2D cursorIcon;
    [SerializeField] int facePlayerSpeed;
    public Vector3 playerDirection;
    [SerializeField] GameObject cameraContainer;
    


    [Header("-----Audio-----")]
    [SerializeField] AudioSource gameMusic;
    [SerializeField] AudioSource menuSounds;
    [SerializeField] AudioClip menuOpen;
    [SerializeField] AudioClip menuClose;
    [SerializeField] int volume;


    #region Initialize
    void Awake()
    {
        instance = this;
        originalTimeScale = Time.timeScale;
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            currState = GameManager.GameStates.mainMenu;
        }
        else
        {
            currState = GameManager.GameStates.play;
        }
        
    }

    void Start()
    {
        playerPauseInput = InputManager.instance.playerInput.PlayerControls.Pause;
        UIPauseInput = InputManager.instance.playerInput.UI.Pause;

        playerPauseInput.started += PauseMenu;
        UIPauseInput.started += UnpauseMenu;

        if(currState != GameManager.GameStates.mainMenu)
        {
            playerDirection = GameManager.instance.playerBodyPositions.playerHead.position - popupMenu.transform.position;
        }
        

        Cursor.SetCursor(cursorIcon, Vector2.zero, CursorMode.ForceSoftware);
    }

    private void OnDisable()
    {
        playerPauseInput.started -= PauseMenu;
        UIPauseInput.started -= UnpauseMenu;
    }
    #endregion

    #region Menus
    private void PauseMenu(InputAction.CallbackContext context)
    {
        currState = GameManager.GameStates.pauseMenu;
        StatePaused();
        menuSounds.PlayOneShot(menuOpen);
        menuActive = menuPause;
        menuActive.SetActive(isPaused);
    }

    private void UnpauseMenu(InputAction.CallbackContext context)
    {
        if (currState != GameManager.GameStates.pauseMenu)
        {
            return;
        }
        else
        {
            StateUnpaused();
            menuSounds.PlayOneShot(menuClose);
            menuActive = menuPause;
            menuActive.SetActive(isPaused);
            OnUnpause.Invoke();
        }
    }

    UnityEvent OnUnpause = new UnityEvent();

    public void SubscribeOnUnpause(UnityAction action) { OnUnpause.AddListener(action); }
    public void UnsubscribeOnUnpause(UnityAction action) { OnUnpause.RemoveListener(action); }

    public GameObject CreatePopup(Transform position, string itemName)
    {
        popupMenu.transform.position = position.position + new Vector3(0, 0.5f, 0);
        popupText.text = itemName;
        popupMenu.SetActive(true);
        return popupMenu;

    }

    public void PopupFacePlayer()
    {
        Quaternion rotation = Quaternion.LookRotation(new Vector3(playerDirection.x, playerDirection.y, playerDirection.z));
        popupMenu.transform.rotation = Quaternion.Lerp(popupMenu.transform.rotation, rotation, Time.deltaTime * facePlayerSpeed);
    }

    public void HidePopup()
    {
        popupMenu.SetActive(false);
    }
    #endregion

    #region Game States
    public void StatePaused()
    {
        
        isPaused = !isPaused;
        InputManager.instance.SwapToPauseInput();
        HUDManager.instance.HideHUD();
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

    }

    public void StateUnpaused()
    {
        currState = GameManager.GameStates.play;
        InputManager.instance.SwapToPlayInput();
        isPaused = !isPaused;
        HUDManager.instance.ShowHUD();
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
        HUDManager.instance.HideHUD();
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

    #endregion
}
