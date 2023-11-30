using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour, IPlayerCombat
{
    InputAction attackInput;
    InputAction focusInput;
    InputAction reloadInput;

    [SerializeField] PlayerController playerController;
    [SerializeField] CameraController cameraController;

    [SerializeField] float maxAimRange;
    [SerializeField] float minAimRange;
    [SerializeField] float recoilSmoothTime;
    Vector3 recoilJumpToAdd;

    [SerializeField] Transform weaponContainer;
    [SerializeField] GameObject startingWeaponPrefab;
    private IWeapon weaponCurrent;
    private IGun gunCurrent;

    Coroutine equippingWeapon;
    Coroutine shotJumping;

    bool isWeaponEquipped;


    #region Initialization
    private void Start()
    {
        InitializeControls();

        EquipWeapon(startingWeaponPrefab);
    }

    void InitializeControls()
    {
        attackInput = InputManager.instance.playerInput.PlayerControls.Attack;
        focusInput = InputManager.instance.playerInput.PlayerControls.Focus;
        reloadInput = InputManager.instance.playerInput.PlayerControls.Reload;

        attackInput.started += AttackStarted;
        attackInput.canceled += AttackCanceled;
        focusInput.started += FocusStarted;
        focusInput.canceled += FocusCanceled;
        reloadInput.started += ReloadStarted;
    }

    void OnDisable()
    {
        attackInput.started -= AttackStarted;
        attackInput.canceled -= AttackCanceled;
        focusInput.started -= FocusStarted;
        focusInput.canceled -= FocusCanceled;
        reloadInput.started -= ReloadStarted;
    }
    #endregion


    private void Update()
    {
        if (isWeaponEquipped)
        {
            AimControl();
        }
    }

    #region Aiming
    void AimControl()
    {
        if (weaponCurrent != null)
        {
            RaycastHit hit;
            Ray centerScreen = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            Vector3 centerPos;

            if (Physics.Raycast(centerScreen, out hit, maxAimRange) && hit.distance > minAimRange)
            {
                Debug.DrawLine(Camera.main.transform.position, hit.point, Color.blue);
                centerPos = hit.point;
            }
            else
            {
                centerPos = Camera.main.transform.position + Camera.main.transform.forward * maxAimRange;
            }

            weaponCurrent.AimAt(centerPos);
        }
    }


    public void FocusStarted(InputAction.CallbackContext context)
    {
        if (weaponCurrent != null)
        {
            weaponCurrent.StartFocus();
        }
    }

    public void FocusCanceled(InputAction.CallbackContext context)
    {
        if (weaponCurrent != null)
        {
            weaponCurrent.StopFocus();
        }
    }

    public void AddShotJump(Vector3 amount)
    {
        if (shotJumping != null)
            StopCoroutine(shotJumping);

        shotJumping = StartCoroutine(ShotJumping(amount));
    }

    IEnumerator ShotJumping(Vector3 amount)
    {
        float startTime = Time.time;
        float percentDone = 0;

        recoilJumpToAdd += amount;
        Vector3 startJumpToAdd = recoilJumpToAdd;

        for(; ; )
        {
            percentDone = (Time.time - startTime) / recoilSmoothTime;

            if (percentDone < 1)
            {
                Vector3 lastRecoilJump = recoilJumpToAdd;
                recoilJumpToAdd = Vector3.Lerp(startJumpToAdd, Vector3.zero, percentDone);
                
                Vector3 jumpDiff = lastRecoilJump - recoilJumpToAdd;

                playerController.AddHorizontalRotation(jumpDiff.x);
                cameraController.AddVerticalRotation(jumpDiff.y);
            }
            else
            {
                playerController.AddHorizontalRotation(recoilJumpToAdd.x);
                cameraController.AddVerticalRotation(recoilJumpToAdd.y);

                recoilJumpToAdd = Vector3.zero;
                shotJumping = null;

                break;
            }

            yield return new WaitForNextFrameUnit();
        }




    }
    #endregion

    #region Attacking
    public void AttackStarted(InputAction.CallbackContext context)
    {
        if (weaponCurrent != null)
        {
            weaponCurrent.StartAttack();
        }
    }

    public void AttackCanceled(InputAction.CallbackContext context)
    {
        if (weaponCurrent != null)
        {
            weaponCurrent.StopAttack();
        }
    }
    #endregion

    #region Ammo Control

    public void ReloadStarted(InputAction.CallbackContext context)
    {
        if (gunCurrent != null)
        {
            gunCurrent.Reload();
        }
    }

    public void UpdateAmmoHUD()
    {
        HUDManager.instance.UpdateAmmoCount(gunCurrent.GetMagAmmo(), gunCurrent.GetReserveAmmo());
    }
    #endregion

    #region Equipping
    public void EquipWeapon(GameObject weapon)
    {
        if (equippingWeapon != null)
            StopCoroutine(equippingWeapon);

        equippingWeapon = StartCoroutine(EquippingWeapon(weapon));
    }

    IEnumerator EquippingWeapon(GameObject weapon)
    {
        // Destroy any existing weapon
        isWeaponEquipped = false;

        foreach (Transform child in weaponContainer)
        {
            Destroy(child.gameObject);
        }

        weaponCurrent = null;
        gunCurrent = null;


        // Create and equip the new weapon
        Instantiate(weapon, weaponContainer);

        weaponCurrent = weaponContainer.GetComponentInChildren<IWeapon>();

        if (weaponCurrent == null)
            yield break;

        if (weaponCurrent is IGun)
        {
            gunCurrent = (IGun)weaponCurrent;

            gunCurrent.SubscribeOnAmmoChange(UpdateAmmoHUD);
            gunCurrent.SubscribeOnShotJump(AddShotJump);
        }

        // Wait for equip to finish
        yield return new WaitForSeconds(weaponCurrent.GetEquipTime());

        weaponCurrent.Equip();
        isWeaponEquipped = true;
    }
    #endregion
}
