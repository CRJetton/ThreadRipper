using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbDetector : MonoBehaviour
{
    private bool canPlayerClimb;

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("Climbable")) canPlayerClimb = true;
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.CompareTag("Climbable")) canPlayerClimb = false;
    }

    public bool GetCanPlayerClimb()
    {
        return canPlayerClimb;
    }
}
