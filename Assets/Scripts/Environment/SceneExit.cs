using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExit : MonoBehaviour, IInteractable, IEnvironment
{
    [SerializeField] string exitName;

    bool canExitScene;

    public string GetName()
    {
        if (canExitScene)
            return exitName;
        else
            return "<s>" + exitName;
    }

    public void Interact(IInteractionController interactionController)
    {
        if (canExitScene)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    public void OnGameGoalComplete()
    {
        canExitScene = true;
    }
}
