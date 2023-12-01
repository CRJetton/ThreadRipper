using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IGun : IWeapon
{
    public void Reload();

    public void SubscribeOnAmmoChange(UnityAction action);
    public void SubscribeOnShotJump(UnityAction<Vector3> action);
    public void SubscribeOnScoping(UnityAction<float, float> action);



    public int GetMagAmmo();
    public int GetReserveAmmo();
}
