using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject menuActive;
    public GameObject menuPause;
    public GameObject menuWin;
    public GameObject menuLose;

    public bool isPaused;
    float originalTimeScale;
    

    void Awake()
    {
        instance = this;
        originalTimeScale  = Time.timeScale;    

    }

    void Update()
    {
        if(Input.GetButtonDown("Cancel") && menuActive == null)
        {
            StatePaused();
            menuActive = menuPause;
            menuActive.SetActive(isPaused);
        }
    }

    public void StatePaused()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void StateUnpaused()
    {
        isPaused = !isPaused;
        Time.timeScale = originalTimeScale;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    

    public void YouLose()
    {
        StatePaused();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }
}
