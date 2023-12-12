using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractionController
{
    void PickUpHealth(HealthPickUp healthPickUp);

    public void PickUpWeapon(WeaponPickup item);

    public void PickUpAmmo(AmmoPickUp _ammoPickup);
}
