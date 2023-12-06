using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyCombat : ICombat
{
    public void AimAt(Vector3 worldPosition);

    public void AttackStarted();
    public void AttackCanceled();
    public void FocusStarted();
    public void FocusCanceled();

    public void ReloadStarted();

    public void Throw(Vector3 velocity, Vector3 angularVelocity);
}
