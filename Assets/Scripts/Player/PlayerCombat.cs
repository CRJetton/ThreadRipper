using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour, IPlayerCombat
{
    [SerializeField] PlayerController playerController;

    [SerializeField] float maxAimRange;
    [SerializeField] float minAimRange;

    [SerializeField] Transform weaponContainer;
    [SerializeField] GameObject startingWeaponPrefab;
    private IWeapon weaponCurrent;
    private IGun gunCurrent;

    Coroutine equippingWeapon;

    bool isWeaponEquipped;


    private void Start()
    {
        EquipWeapon(startingWeaponPrefab);
    }


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

    public void AddShotJump(Vector3 amount)
    {
        playerController.AddLookRotation(amount);
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
