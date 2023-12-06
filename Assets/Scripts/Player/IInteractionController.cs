using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractionController
{
    public void PickUpWeapon(WeaponPickup item);

    public void PickUpHealth(float amount);
}
