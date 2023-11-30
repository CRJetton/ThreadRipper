using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour, ICombat
{
    [SerializeField] Transform weaponContainer;
    [SerializeField] GameObject startingWeaponPrefab;
    private IWeapon weaponCurrent;
    private IGun gunCurrent;

    bool isWeaponEquipped;

    Coroutine equippingWeapon;

    void Start()
    {
        EquipWeapon(startingWeaponPrefab);
    }


    public void AimAt(Vector3 worldPosition)
    {
        weaponCurrent.AimAt(worldPosition);
    }

    public void AttackCanceled()
    {
        weaponCurrent.AttackCanceled();
    }

    public void AttackStarted()
    {
        weaponCurrent.AttackStarted();
    }

    public void FocusStarted()
    {
        weaponCurrent.FocusStarted();
    }

    public void FocusCanceled()
    {
        weaponCurrent.FocusCanceled();
    }

    public void ReloadStarted()
    {
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
        isWeaponEquipped = false;

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
        isWeaponEquipped = true;
    }
}
