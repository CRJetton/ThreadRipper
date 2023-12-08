using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    InputAction lookInput;
    InputAction moveInput;
    InputAction sprintInput;

    [Header("Componenets")]
    [SerializeField] PlayerController player;
    [SerializeField] Camera[] cameras;
    [SerializeField] PlayerGroundDetector playerGroundDetector;

    [Header("")]
    [SerializeField] private int lockVertMin;
    [SerializeField] private int lockVertMax;
    [SerializeField] private bool invertY;
    private Vector3 currLookRot;

    float defaultFOV;
    float currentZoomFactor = 1;

    Coroutine zooming;

    [Header("_____HeadBobbing_____")]
    private float currBobSpeed;
    [SerializeField] private float defaultBobSpeed;
    [SerializeField] private float sprintBobSpeed;
    [Range(0, 0.1f)][SerializeField] private float headBobDistanceHoriz;
    [Range(0,0.1f)][SerializeField] private float headBobDistanceVert;
    private Vector3 origPos;

    private void Awake()
    {
        origPos = transform.localPosition;
    }

    private void Start()
    {
        lookInput = InputManager.instance.playerInput.PlayerControls.Look;
        moveInput = InputManager.instance.playerInput.PlayerControls.Move;
        sprintInput = InputManager.instance.playerInput.PlayerControls.Sprint;

        defaultFOV = cameras[0].fieldOfView;
    }

    private void LateUpdate()
    {
        AddVerticalRotation(lookInput.ReadValue<Vector2>().y * (invertY ? -1 : 1) * SettingsManager.instance.lookSensitivity * Time.deltaTime);
        HandleHeadBob();
    }

    private void HandleHeadBob()
    {
        // Adjust speeds of bob based on movement state.
        if (sprintInput.IsPressed() && currBobSpeed != sprintBobSpeed)
        {
            currBobSpeed = sprintBobSpeed;
        }
        else if (!sprintInput.IsPressed() && currBobSpeed != defaultBobSpeed)
        {
            currBobSpeed = defaultBobSpeed;
        }

        if (moveInput.ReadValue<Vector2>().magnitude != 0)
        {
            // Use trig functions to create back and forth movements up and down over time.
            Vector3 bobTowards = origPos;
            bobTowards.x += Mathf.Cos(Time.time * currBobSpeed) * headBobDistanceHoriz;
            bobTowards.y += Mathf.Sin(Time.time * currBobSpeed) * headBobDistanceVert;
            bobTowards *= moveInput.ReadValue<Vector2>().magnitude;
            transform.localPosition = bobTowards;
        }
        else
        {
            // Reset camera position when not moving.
            transform.localPosition = origPos;
        }
    }

    public void AddVerticalRotation(float _vertLookVec)
    {
        // Camera rotation
        currLookRot.x -= _vertLookVec;
        currLookRot.x = Mathf.Clamp(currLookRot.x, lockVertMin, lockVertMax);
        currLookRot.y = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(currLookRot);
    }

    public IEnumerator Crouch(float _targetHeight, float _totalTime)
    {
        float currTime = 0f;
        float startHeight = transform.localPosition.y;

        while (currTime < _totalTime)
        {
            Vector3 moveTo = transform.localPosition;
            moveTo.y = Mathf.Lerp(startHeight, _targetHeight, currTime / _totalTime);
            transform.localPosition = moveTo;
            currTime += Time.deltaTime;
            yield return null;
        }

        Vector3 targetPos = transform.localPosition;
        targetPos.y = _targetHeight;
        transform.localPosition = targetPos;
    }

    public void Zoom(float zoomFactor, float zoomTime)
    {
        if (zooming != null)
            StopCoroutine(zooming);

        zooming = StartCoroutine(Zooming(zoomFactor, zoomTime));
    }

    IEnumerator Zooming(float zoomFactor, float zoomTime)
    {
        float startTime = Time.time;
        float percentDone = 0;
        float startZoomFactor = currentZoomFactor;

        for(; ; )
        {
            percentDone = (Time.time - startTime) / zoomTime;

            if (percentDone < 1)
            {
                currentZoomFactor = Mathf.Lerp(startZoomFactor, zoomFactor, percentDone);

                UpdateCameraFOV();

                yield return new WaitForNextFrameUnit();
            }
            else
            {
                currentZoomFactor = zoomFactor;

                UpdateCameraFOV();

                zooming = null;
                break;
            }
        }
    }

    void UpdateCameraFOV()
    {
        foreach(Camera cam in cameras)
        {
            cam.fieldOfView = defaultFOV * currentZoomFactor;
        }
    }
}
