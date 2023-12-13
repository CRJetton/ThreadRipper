using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyPositions : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerCenter;
    [SerializeField] private float standingCenterOffset;
    [SerializeField] private float crouchingCenterOffset;
    public Transform playerHead;

    private void Awake()
    {
        playerCenter.localPosition = characterController.center;
        playerCenter.localPosition += new Vector3(0, standingCenterOffset, 0);
    }

    public void Crouch()
    {
        playerCenter.localPosition = characterController.center;
        playerCenter.localPosition += new Vector3(0, standingCenterOffset, 0);
    }

    public Vector3 GetPlayerCenter()
    {
        return playerCenter.position;
    }
}
