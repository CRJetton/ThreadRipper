using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    public void AimAt(Vector3 targetPosition);
    public void StartAttack();
    public void StopAttack();
    public void StartFocus();
    public void StopFocus();

    public void Equip();
    public float GetEquipTime();
}
