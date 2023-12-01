using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    InputAction lookInput;

    [Header("Componenets")]
    [SerializeField] PlayerController player;
    [SerializeField] Camera[] cameras;

    [Header("")]
    [SerializeField] private int lockVertMin;
    [SerializeField] private int lockVertMax;
    [SerializeField] private bool invertY;
    private Vector3 currLookRot;

    float defaultFOV;
    float currentZoomFactor;

    Coroutine zooming;

    private void Start()
    {
        lookInput = InputManager.instance.playerInput.PlayerControls.Look;

        defaultFOV = cameras[0].fieldOfView;
    }

    private void LateUpdate()
    {
        AddVerticalRotation(lookInput.ReadValue<Vector2>().y * (invertY ? -1 : 1) * player.GetLookSensitivity() * Time.deltaTime);
    }

    public void AddVerticalRotation(float _vertLookVec)
    {
        // Camera rotation
        currLookRot.x -= _vertLookVec;
        currLookRot.x = Mathf.Clamp(currLookRot.x, lockVertMin, lockVertMax);
        currLookRot.y = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(currLookRot);
    }

    public void Translate(Vector3 _direction)
    {
        transform.position += _direction;
    }


    public void Zoom(float zoomFactor, float zoomTime)
    {
        if (zooming != null)
            StopCoroutine(zooming);

        zooming = StartCoroutine(Zooming(zoomFactor, zoomTime));
    }

    IEnumerator Zooming(float zoomFactor, float zoomTime)
    {
        yield return null;
    }
}
