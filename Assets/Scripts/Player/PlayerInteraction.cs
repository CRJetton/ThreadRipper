using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour, IInteractionController
{
    InputAction interactInput;

    [Header("Components")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] PlayerCombat playerCombat;
    [SerializeField] PlayerBodyPositions bodyPositions;
    [SerializeField] InteractableDetector interactableDetector;

    IInteractable currentInteractable;

    #region Initialization
    private void Start()
    {
        interactInput = InputManager.instance.playerInput.PlayerControls.Interact;

        interactInput.started += Interact;

        interactableDetector.SubscribeOnTouch(OnInteractablesChange);
        interactableDetector.SubscribeOnStopTouch(OnInteractablesChange);
    }

    private void OnDisable()
    {
        interactInput.started -= Interact;
    }
    #endregion


    #region Interact
    void Interact(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }

    void OnInteractablesChange()
    {
        GameObject interactable = interactableDetector.GetClosestInteractable(bodyPositions.playerHead.position);

        if (interactable != null)
        {
            currentInteractable = interactable.GetComponent<IInteractable>();
            UIManager.instance.CreatePopup(interactable.transform, interactable.name);
        }
        else
        {
            currentInteractable = null;
        }
    }
    #endregion

    #region Specific Interactions
    public void PickUpWeapon(WeaponPickup item)
    {
        if (playerCombat.GetCanEquipWeapon())
        {
            interactableDetector.RemoveInteractable(item);

            playerCombat.EquipWeapon(item);
            item.PickedUp();
        }
    }

    public void PickUpHealth(HealthPickUp healingPack)
    {
        playerController.AddHP(healingPack.GetHealAmount());
        Destroy(healingPack.gameObject);
    }
    #endregion
}
