using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerVaultDetector : MonoBehaviour
{
    private bool canPlayerVault;
    private List<Collider> vaultContacts = new List<Collider>();

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.isTrigger) return;
        if (_other.CompareTag("Vaultable"))
        {
            vaultContacts.Add( _other );
            canPlayerVault = true;
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.isTrigger) return;
        if (_other.CompareTag("Vaultable"))
        {
            vaultContacts.Remove( _other );
            if (vaultContacts.Count() <= 0) canPlayerVault = false;
        }
    }

    public bool GetCanPlayerVault()
    {
        return canPlayerVault;
    }
}
