using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDetector : MonoBehaviour
{
    List<IInteractable> interactablesTouching = new List<IInteractable>();


    public IInteractable GetClosestInteractable(Vector3 origin)
    {
        float closestDist = Mathf.Infinity;
        float currentDist;
        IInteractable closestInteractable = null;

        foreach (IInteractable interactable in interactablesTouching)
        {
            currentDist = interactable.GetSquareDistance(origin);

            if (currentDist < closestDist)
            {
                closestDist = currentDist;
                closestInteractable = interactable;
            }
        }

        return closestInteractable;
    }


    public void RemoveInteractable(IInteractable interactable)
    {
        interactablesTouching.Remove(interactable);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null)
        {
            interactablesTouching.Add(interactable);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
            return;

        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null)
        {
            interactablesTouching.Remove(interactable);
        }
    }
}
