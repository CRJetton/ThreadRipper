using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] Texture2D cursorIcon;

    void Start()
    {
        Cursor.SetCursor(cursorIcon, Vector2.zero, CursorMode.ForceSoftware);
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void StartGame()
    {
        SceneManager.LoadScene(2);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
