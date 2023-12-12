using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerClimbDetector : MonoBehaviour
{
    private bool canPlayerClimb;
    private List<Collider> climbContacts = new List<Collider>();

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.isTrigger || _other.GetComponent<CharacterController>()) return;
        if (_other.CompareTag("Climbable"))
        {
            climbContacts.Add( _other );
            canPlayerClimb = true;
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.isTrigger) return;
        if (_other.CompareTag("Climbable"))
        {
            climbContacts.Remove( _other );
            if (climbContacts.Count() <= 0) canPlayerClimb = false;
        }
    }

    public bool GetCanPlayerClimb()
    {
        return canPlayerClimb;
    }
}
