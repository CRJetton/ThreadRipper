using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickUp : MonoBehaviour, IInteractable
{
    [SerializeField] private int healAmount;

    //public enum HealingType
    //{
    //    Largepack,   // Heals 200 HP
    //    Mediumpack,  // Heals 75 HP
    //    Smallpack,    // Heals 45 HP
    //}

    //[SerializeField] HealingType healingType;

    public void Interact(IInteractionController interactionController)
    {
        interactionController.PickUpHealth(this);
    }

    public int GetHealAmount()
    {
        return healAmount;
    }
    
   

    //public HealingType GetHealingType()
    //{
    //    return healingType;
    //}

    //public void HealPlayer(PlayerController playerController)
    //{
    //    switch (healingType)
    //    {
    //        case HealingType.Largepack:
    //            playerController.AddHP(150);
    //            break;
    //        case HealingType.Mediumpack:
    //            playerController.AddHP(60);
    //            break;
    //        case HealingType.Smallpack:
    //            playerController.AddHP(25);
    //            break;
    //    }
    //}
}



