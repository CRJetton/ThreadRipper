using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerCombat
{
    public void AttackStarted(InputAction.CallbackContext context);

    public void AttackCanceled(InputAction.CallbackContext context);

    public void FocusStarted(InputAction.CallbackContext context);

    public void FocusCanceled(InputAction.CallbackContext context);

    public void ReloadStarted(InputAction.CallbackContext context);
}
