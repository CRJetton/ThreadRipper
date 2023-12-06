using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDetector : MonoBehaviour
{
    List<GameObject> interactablesTouching = new List<GameObject>();


    public IInteractable GetClosestInteractable(Vector3 origin)
    {
        float closestDist = Mathf.Infinity;
        float currentDist;
        GameObject closestInteractable = null;

        foreach (GameObject interactable in interactablesTouching)
        {
            currentDist = Vector3.SqrMagnitude(origin - interactable.transform.position);

            if (currentDist < closestDist)
            {
                closestDist = currentDist;
                closestInteractable = interactable;
            }
        }

        if (closestInteractable != null)
            return closestInteractable.GetComponent<IInteractable>();
        else
            return null;
    }


    public void RemoveInteractable(IInteractable interactable)
    {
        for (int i = 0; i < interactablesTouching.Count; i++)
        {
            if (interactablesTouching[i].GetComponent<IInteractable>() == interactable)
            {
                interactablesTouching.RemoveAt(i);
                break;
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null)
        {
            interactablesTouching.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
            return;

        IInteractable interactable = other.GetComponent<IInteractable>();

        if (interactable != null)
        {
            interactablesTouching.Remove(other.gameObject);
        }
    }
}
