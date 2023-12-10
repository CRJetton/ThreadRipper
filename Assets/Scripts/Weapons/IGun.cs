using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IGun : IWeapon
{
    public void Reload();

    public void ContainerManuallyRotated(Vector2 amount);

    public void SubscribeOnAmmoChange(UnityAction action);
    public void SubscribeOnShotJump(UnityAction<Vector3> action);
    public void SubscribeOnScoping(UnityAction<float, float> action);


    public void InitializeAmmo(int magAmmo, int reserveAmmo);

    public int GetMagAmmo();
    public int GetReserveAmmo();
    public int GetMagAmmoCapacity();
    public int GetReserveAmmoCapacity();

    public void SetMagAmmo(int ammo);
    public void SetReserveAmmo(int ammo);
}
