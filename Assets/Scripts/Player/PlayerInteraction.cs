using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour, IInteractionController
{
    InputAction interactInput;

    [Header("Components")]
    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] InteractableDetector interactableDetector;


    #region Initialization
    private void Start()
    {
        interactInput = InputManager.instance.playerInput.PlayerControls.Interact;

        interactInput.started += Interact;
    }

    private void OnDisable()
    {
        interactInput.started -= Interact;
    }
    #endregion


    #region Interact
    void Interact(InputAction.CallbackContext context)
    {
        IInteractable interactable = interactableDetector.GetClosestInteractable(transform.position);

        if (interactable != null)
        {
            interactable.Interact(this);
        }
    }
    #endregion

    #region Specific Interactions

    public void PickUpItem(ItemPickup item)
    {
        if (playerCombat.GetCanEquipWeapon())
        {
            interactableDetector.RemoveInteractable(item);

            playerCombat.EquipWeapon(item);
            item.PickedUp();
        }
    }

    #endregion
}
