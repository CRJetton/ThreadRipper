using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/* LOG
 *
 * Need to communicate with i combat interface
 */

public class PlayerController : MonoBehaviour
{
    // Movement
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float defaultMoveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float lookSensitivity;
    private float currMoveSpeed;
    private Vector3 move;

    // Jumping & gravity
    [SerializeField] private float jumpHeight;
    [SerializeField] private float gravity;

    // Vaulting
    [SerializeField] private float vaultTime;
    [SerializeField] float vaultForward;
    [SerializeField] float vaultUp;

    private void Start()
    {
        // Cursor handling
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        currMoveSpeed = defaultMoveSpeed;
    }

    // Update is called once per frame
    private void Update()
    {
        move = (InputManager.instance.GetMoveInput().x * transform.right * currMoveSpeed)
            + (move.y * transform.up)
            + (InputManager.instance.GetMoveInput().y * transform.forward * currMoveSpeed);

        characterController.Move(move * Time.deltaTime);
        transform.Rotate(Vector3.up * InputManager.instance.GetLookInput().x * lookSensitivity * Time.deltaTime);

        //Gravity
        if (move.y > gravity) move.y += gravity * Time.deltaTime;
    }

    public IEnumerator Vault(float _vaultTime)
    {
        float time = 0;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos
            + (transform.forward * (vaultForward))
            + (transform.up * vaultUp);

        while (time < _vaultTime)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, time / _vaultTime);

            time += Time.deltaTime;
            yield return null;
        }
    }

    public float GetLookSensitivity()
    {
        return lookSensitivity;
    }

    public float GetVaultTime()
    {
        return vaultTime;
    }

    public float GetJumpHeight()
    {
        return jumpHeight;
    }

    public float GetSprintSpeed()
    {
        return sprintSpeed;
    }

    public float GetDefaultSpeed()
    {
        return defaultMoveSpeed;
    }

    public void SetMoveSpeed(float _speed)
    {
        currMoveSpeed = _speed;
    }

    public void SetMoveUp(float _up)
    {
        move.y = _up;
    }
}
