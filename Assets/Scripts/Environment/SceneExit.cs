using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExit : MonoBehaviour, IInteractable
{
    [SerializeField] string exitName;

    bool canExitScene = true;

    public string GetName()
    {
        return exitName;
    }

    public void Interact(IInteractionController interactionController)
    {
        if (canExitScene)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
