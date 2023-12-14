using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
   public void Resume()
    {
        UIManager.instance.StateUnpaused();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Continue()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        UIManager.instance.StateUnpaused();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        UIManager.instance.StateUnpaused();
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(1);
    }
    

    public void Respawn()
    {
        GameManager.instance.playerController.OnRespawn();
        UIManager.instance.StateUnpaused();
    }

    public void Settings()
    {
        UIManager.instance.menuPause.SetActive(false);
        UIManager.instance.menuSettings.SetActive(true);
        
    }

    public void Back()
    {
        UIManager.instance.menuSettings.SetActive(false);
        UIManager.instance.menuPause.SetActive(true);
        UIManager.instance.currState = GameManager.GameStates.pauseMenu;

    }
   

    

}
