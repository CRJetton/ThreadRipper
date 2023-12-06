using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour, ICombat
{
    InputAction attackInput;
    InputAction focusInput;
    InputAction reloadInput;
    InputAction lookInput;
    InputAction throwInput;

    [SerializeField] PlayerController playerController;
    [SerializeField] CameraController cameraController;

    [SerializeField] LayerMask aimIgnoreLayer;

    [SerializeField] float maxAimRange;
    [SerializeField] float minAimRange;
    [SerializeField] float recoilSmoothTime;
    Vector3 recoilJumpToAdd;
    Vector3 aimWorldPos;

    [SerializeField, Range(0, 50)] float throwSpeed;
    [SerializeField, Range(-100, 100)] float throwAngularSpeed;
    [SerializeField, Range(-1f, 1f)] float throwAngle;

    [SerializeField] Transform weaponContainer;
    [SerializeField] GameObject startingWeaponPrefab;
    private IWeapon weaponCurrent;
    private IGun gunCurrent;

    Coroutine equippingWeapon;
    Coroutine shotJumping;

    bool isWeaponEquipped;
    bool canEquipWeaopn = true;


    #region Initialization
    private void Start()
    {
        InitializeControls();

        if (startingWeaponPrefab != null)
            EquipWeapon(startingWeaponPrefab);
    }

    void InitializeControls()
    {
        attackInput = InputManager.instance.playerInput.PlayerControls.Attack;
        focusInput = InputManager.instance.playerInput.PlayerControls.Focus;
        reloadInput = InputManager.instance.playerInput.PlayerControls.Reload;
        lookInput = InputManager.instance.playerInput.PlayerControls.Look;
        throwInput = InputManager.instance.playerInput.PlayerControls.Throw;

        attackInput.started += AttackStarted;
        attackInput.canceled += AttackCanceled;
        focusInput.started += FocusStarted;
        focusInput.canceled += FocusCanceled;
        reloadInput.started += ReloadStarted;
        throwInput.started += ThrowStarted;
    }

    void OnDisable()
    {
        attackInput.started -= AttackStarted;
        attackInput.canceled -= AttackCanceled;
        focusInput.started -= FocusStarted;
        focusInput.canceled -= FocusCanceled;
        reloadInput.started -= ReloadStarted;
        throwInput.started -= ThrowStarted;  
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

            if (Physics.Raycast(centerScreen, out hit, maxAimRange, ~aimIgnoreLayer))
            {
                if (hit.distance < minAimRange)
                    return;

                Debug.DrawLine(Camera.main.transform.position, hit.point, Color.blue);
                aimWorldPos = hit.point;
            }
            else
            {
                aimWorldPos = Camera.main.transform.position + Camera.main.transform.forward * maxAimRange;
            }

            weaponCurrent.AimAt(aimWorldPos);
        }

        if (gunCurrent != null)
        {
            gunCurrent.ContainerManuallyRotated(lookInput.ReadValue<Vector2>() * SettingsManager.instance.lookSensitivity * Time.deltaTime);
        }
    }


    public void FocusStarted(InputAction.CallbackContext context)
    {
        if (weaponCurrent != null)
        {
            weaponCurrent.FocusStarted();
        }
    }

    public void FocusCanceled(InputAction.CallbackContext context)
    {
        if (weaponCurrent != null)
        {
            weaponCurrent.FocusCanceled();
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

        for (; ; )
        {
            percentDone = (Time.time - startTime) / recoilSmoothTime;

            if (percentDone < 1)
            {
                Vector3 lastRecoilJump = recoilJumpToAdd;
                recoilJumpToAdd = Vector3.Lerp(startJumpToAdd, Vector3.zero, percentDone);

                Vector3 jumpDiff = lastRecoilJump - recoilJumpToAdd;

                playerController.Look(jumpDiff.x);
                cameraController.AddVerticalRotation(jumpDiff.y);
            }
            else
            {
                playerController.Look(recoilJumpToAdd.x);
                cameraController.AddVerticalRotation(recoilJumpToAdd.y);

                recoilJumpToAdd = Vector3.zero;
                shotJumping = null;

                break;
            }

            yield return new WaitForNextFrameUnit();
        }
    }

    public void ChangeScopeZoom(float zoomFactor, float zoomTime)
    {
        cameraController.Zoom(zoomFactor, zoomTime);
    }
    #endregion

    #region Attacking
    public void AttackStarted(InputAction.CallbackContext context)
    {
        if (weaponCurrent != null)
        {
            weaponCurrent.AttackStarted();
        }
    }

    public void AttackCanceled(InputAction.CallbackContext context)
    {
        if (weaponCurrent != null)
        {
            weaponCurrent.AttackCanceled();
        }
    }

    public void ThrowStarted(InputAction.CallbackContext context)
    {
        ThrowWeapon();
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

    #region Equipping, Dropping, Throwing
    public void EquipWeapon(GameObject prefab)
    {
        if (equippingWeapon != null)
            StopCoroutine(equippingWeapon);

        equippingWeapon = StartCoroutine(EquippingWeapon(prefab));
    }

    public void EquipWeapon(ItemPickup pickup)
    {
        if (equippingWeapon != null)
            StopCoroutine(equippingWeapon);

        equippingWeapon = StartCoroutine(EquippingWeapon(pickup));
    }

    IEnumerator EquippingWeapon(ItemPickup pickup)
    {
        canEquipWeaopn = false;

        // Drop any existing weapon
        DropWeapon();


        // Create and equip the new weapon
        Instantiate(pickup.GetPlayerItemPrefab(), weaponContainer);

        weaponCurrent = weaponContainer.GetComponentInChildren<IWeapon>();

        if (weaponCurrent == null)
            yield break;

        if (weaponCurrent is IGun)
        {
            gunCurrent = (IGun)weaponCurrent;

            gunCurrent.SubscribeOnAmmoChange(UpdateAmmoHUD);
            gunCurrent.SubscribeOnShotJump(AddShotJump);
            gunCurrent.SubscribeOnScoping(ChangeScopeZoom);
            gunCurrent.InitializeAmmo(pickup.GetSaveValue1(), pickup.GetSaveValue2());
        }

        // Wait for equip to finish
        yield return new WaitForSeconds(weaponCurrent.GetEquipTime());

        weaponCurrent.Equip();
        isWeaponEquipped = true;
        canEquipWeaopn = true;
    }

    IEnumerator EquippingWeapon(GameObject prefab)
    {
        canEquipWeaopn = false;

        // Drop any existing weapon
        DropWeapon();


        // Create and equip the new weapon
        Instantiate(prefab, weaponContainer);

        weaponCurrent = weaponContainer.GetComponentInChildren<IWeapon>();

        if (weaponCurrent == null)
            yield break;

        if (weaponCurrent is IGun)
        {
            gunCurrent = (IGun)weaponCurrent;

            gunCurrent.SubscribeOnAmmoChange(UpdateAmmoHUD);
            gunCurrent.SubscribeOnShotJump(AddShotJump);
            gunCurrent.SubscribeOnScoping(ChangeScopeZoom);
        }

        // Wait for equip to finish
        yield return new WaitForSeconds(weaponCurrent.GetEquipTime());

        weaponCurrent.Equip();
        isWeaponEquipped = true;
        canEquipWeaopn = true;
    }

    private void DropWeapon()
    {
        if (weaponCurrent == null)
            return;

        isWeaponEquipped = false;

        weaponCurrent.Drop();

        weaponCurrent = null;
        gunCurrent = null;
    }

    private void ThrowWeapon()
    {
        if (weaponCurrent == null)
            return;

        isWeaponEquipped = false;

        Vector3 velocity = weaponCurrent.getObject().transform.forward;
        velocity.y += throwAngle;
        velocity = velocity.normalized;
        velocity *= throwSpeed;

        Vector3 angularVelocity = weaponCurrent.getObject().transform.right * throwAngularSpeed;

        weaponCurrent.Throw(velocity, angularVelocity);

        weaponCurrent = null;
        gunCurrent = null;
    }
    #endregion

    #region Getters
    public bool GetCanEquipWeapon() { return canEquipWeaopn; }
    #endregion
}
