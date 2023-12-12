using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickUp : MonoBehaviour, IInteractable
{
    [SerializeField] string itemName;
    [SerializeField] private int ammoAmount;

    public void Interact(IInteractionController _interactController)
    {
        //_interactController.PickUpAmmo(this);
    }

    public string GetName()
    {
        return itemName;
    }

    public int GetAmmoAmount()
    {
        return ammoAmount;
    }
}
