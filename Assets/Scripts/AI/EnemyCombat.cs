using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour, ICombat
{
    [SerializeField] Transform weaponContainer;
    [SerializeField] GameObject startingWeaponPrefab;
    private IWeapon weaponCurrent;
    private IGun gunCurrent;

    Coroutine equippingWeapon;

    void Start()
    {
        EquipWeapon(startingWeaponPrefab);
    }


    public void AimAt(Vector3 worldPosition)
    {
        if (weaponCurrent != null)
        weaponCurrent.AimAt(worldPosition);
    }

    public void AttackCanceled()
    {
        if (weaponCurrent != null)
            weaponCurrent.AttackCanceled();
    }

    public void AttackStarted()
    {
        if (weaponCurrent != null)
            weaponCurrent.AttackStarted();
    }

    public void FocusStarted()
    {
        if (weaponCurrent != null)
            weaponCurrent.FocusStarted();
    }

    public void FocusCanceled()
    {
        if (weaponCurrent != null)
            weaponCurrent.FocusCanceled();
    }

    public void ReloadStarted()
    {
        if (weaponCurrent != null)
            gunCurrent.Reload();
    }


    public void EquipWeapon(GameObject prefab)
    {
        if (equippingWeapon != null)
            StopCoroutine(equippingWeapon);

        equippingWeapon = StartCoroutine(EquippingWeapon(prefab));
    }

    IEnumerator EquippingWeapon(GameObject prefab)
    {
        // Destroy any existing weapon
        foreach (Transform child in weaponContainer)
        {
            Destroy(child.gameObject);
        }

        weaponCurrent = null;
        gunCurrent = null;


        // Create and equip the new weapon
        Instantiate(prefab, weaponContainer);

        weaponCurrent = weaponContainer.GetComponentInChildren<IWeapon>();

        if (weaponCurrent == null)
            yield break;

        if (weaponCurrent is IGun)
            gunCurrent = (IGun) weaponCurrent;

        // Wait for equip to finish
        yield return new WaitForSeconds(weaponCurrent.GetEquipTime());

        weaponCurrent.Equip();
    }
}
