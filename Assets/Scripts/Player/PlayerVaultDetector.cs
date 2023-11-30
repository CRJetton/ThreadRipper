using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVaultDetector : MonoBehaviour
{
    private bool canPlayerVault;

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("Vaultable")) canPlayerVault = true;
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("Vaultable")) canPlayerVault = false;
    }

    public bool GetCanPlayerVault()
    {
        return canPlayerVault;
    }
}
