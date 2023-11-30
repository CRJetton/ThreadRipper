using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundDetector : MonoBehaviour
{
    private bool isPlayerGrounded;

    private void OnTriggerEnter(Collider _other)
    {
        isPlayerGrounded = true;
    }

    private void OnTriggerExit(Collider _other)
    {
        isPlayerGrounded = false;
    }

    public bool GetIsPlayerGrounded()
    {
        return isPlayerGrounded;
    }
}
