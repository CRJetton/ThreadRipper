using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{
    public void AimAt(Vector3 targetPosition);
    public void AttackStarted();
    public void AttackCanceled();
    public void FocusStarted();
    public void FocusCanceled();

    public void Equip();
    public float GetEquipTime();

    public void Drop();
}
