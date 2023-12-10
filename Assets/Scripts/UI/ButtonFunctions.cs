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

}
