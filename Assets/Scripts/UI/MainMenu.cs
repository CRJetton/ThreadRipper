using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject mainSettingsMenu;
    [SerializeField] Texture2D cursorIcon;

    void Start()
    {
        Cursor.SetCursor(cursorIcon, Vector2.zero, CursorMode.ForceSoftware);
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(2);
    }

    public void MainMenuSettings()
    {
        mainMenu.SetActive(false);
        mainSettingsMenu.SetActive(true);

    }

    public void Back()
    {
        mainSettingsMenu.SetActive(false);
        mainMenu.SetActive(true);
        
    }
}
