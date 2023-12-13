using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispenser : MonoBehaviour, IInteractable
{
    [SerializeField] private List<GameObject> dispensables = new List<GameObject>();
    [SerializeField] private Transform ejectPos;
    [SerializeField] private string interactibleName;

    public void Interact(IInteractionController _interactionController)
    {
        _interactionController.UseDispenser(this);
    }

    public void Dispense()
    {
        int dispensableIndex = Random.Range(0, dispensables.Count - 1);
        GameObject dispensable = dispensables[dispensableIndex];
        Instantiate(dispensable, ejectPos);
    }

    public string GetName()
    {
        return interactibleName;
    }
}
