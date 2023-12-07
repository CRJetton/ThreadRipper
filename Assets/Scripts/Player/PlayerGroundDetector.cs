using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerGroundDetector : MonoBehaviour
{
    private bool isPlayerGrounded;
    private List<Collider> groundContacts = new List<Collider>();

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.isTrigger) return;
        groundContacts.Add(_other);
        isPlayerGrounded = true;
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.isTrigger) return;
        groundContacts.Remove(_other);
        if (groundContacts.Count() <= 0) isPlayerGrounded = false;
    }

    public bool GetIsPlayerGrounded()
    {
        return isPlayerGrounded;
    }
}
