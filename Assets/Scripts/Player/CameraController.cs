using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    InputAction lookInput;

    [SerializeField] PlayerController player;
    [SerializeField] private int lockVertMin;
    [SerializeField] private int lockVertMax;
    [SerializeField] private bool invertY;
    private Vector3 currLookRot;

    private void Start()
    {
        lookInput = InputManager.instance.playerInput.PlayerControls.Look;
    }

    private void LateUpdate()
    {
        AddVerticalRotation(lookInput.ReadValue<Vector2>().y * player.GetLookSensitivity() * Time.deltaTime);
    }

    public void AddVerticalRotation(float _vertLookVec)
    {
        // Camera rotation
        currLookRot.x -= _vertLookVec;
        currLookRot.x = Mathf.Clamp(currLookRot.x, lockVertMin, lockVertMax);
        currLookRot.y = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(currLookRot);
    }
}
