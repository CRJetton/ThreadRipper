using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] private int lockVertMin;
    [SerializeField] private int lockVertMax;
    [SerializeField] private bool invertY;
    private Vector3 currLookRot;

    private void LateUpdate()
    {
        float lookYInput = InputManager.instance.GetLookInput().y * player.GetLookSensitivity() * Time.deltaTime;

        AddVerticalRotation(lookYInput);
    }

    public void AddVerticalRotation(float amount)
    {
        // Camera rotation
        currLookRot.x -= amount;
        currLookRot.x = Mathf.Clamp(currLookRot.x, lockVertMin, lockVertMax);
        currLookRot.y = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(currLookRot);
    }
}
