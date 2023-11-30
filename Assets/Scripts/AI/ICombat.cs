using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombat
{
    public void AimAt(Vector3 worldPosition);

    public void AttackStarted();
    public void AttackCanceled();
    public void FocusStarted();
    public void FocusCanceled();

    public void ReloadStarted();

    public void EquipWeapon(GameObject prefab);
}
