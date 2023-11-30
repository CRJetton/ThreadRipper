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
        // Camera rotation
        currLookRot.x -= InputManager.instance.GetLookInput().y * player.GetLookSensitivity() * Time.deltaTime;
        currLookRot.x = Mathf.Clamp(currLookRot.x, lockVertMin, lockVertMax);
        currLookRot.y = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(currLookRot);
    }
}
