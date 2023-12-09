using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDetector : MonoBehaviour
{
    [SerializeField] List<GameObject> interactablesTouching = new List<GameObject>();

    public IInteractable GetClosestInteractable(Vector3 origin)
    {
        float closestDist = Mathf.Infinity;
        float currentDist;
        GameObject closestInteractable = null;

        foreach (GameObject interactable in interactablesTouching)
        {
            // Check if the GameObject is null (destroyed)
            if (interactable == null)
            {
                continue; // Skip to the next iteration of the loop
            }

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
        for (int i = interactablesTouching.Count - 1; i >= 0; i--)
        {
            if (interactablesTouching[i] == null)
            {
                interactablesTouching.RemoveAt(i);
                continue;
            }

            IInteractable currentInteractable = interactablesTouching[i].GetComponent<IInteractable>();

            if (currentInteractable == interactable)
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

        interactablesTouching.Remove(other.gameObject);
    }
}
