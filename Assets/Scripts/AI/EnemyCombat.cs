using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour, IEnemyCombat
{
    [Header("Combat")]
    [SerializeField] Transform weaponContainer;
    [SerializeField] GameObject startingWeapon;
    private IWeapon weaponCurrent;
    private IGun gunCurrent;

    [Header("Throwing")]
    [SerializeField, Range(0, 50)] float throwSpeed;
    [SerializeField, Range(-100, 100)] float throwAngularSpeed;
    [SerializeField, Range(-1f, 1f)] float throwAngle;

    Coroutine equippingWeapon;

    void Start()
    {
        EquipWeapon(startingWeapon.GetComponent<WeaponPickup>());
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

    public void EquipWeapon(WeaponPickup pickup)
    {
        if (equippingWeapon != null)
            StopCoroutine(equippingWeapon);

        equippingWeapon = StartCoroutine(EquippingWeapon(pickup));
    }

    IEnumerator EquippingWeapon(WeaponPickup pickup)
    {
        // Destroy any existing weapon
        foreach (Transform child in weaponContainer)
        {
            Destroy(child.gameObject);
        }

        weaponCurrent = null;
        gunCurrent = null;


        // Create and equip the new weapon
        Instantiate(pickup.GetEnemyWeapon().weaponPrefab, weaponContainer);

        weaponCurrent = weaponContainer.GetComponentInChildren<IWeapon>();

        if (weaponCurrent == null)
            yield break;

        weaponCurrent.SetStats(pickup.GetEnemyWeapon());

        if (weaponCurrent is IGun)
        {
            gunCurrent = (IGun)weaponCurrent;

            gunCurrent.InitializeAmmo(gunCurrent.GetMagAmmoCapacity(), Random.Range(0, gunCurrent.GetMagAmmoCapacity()));
        }

        // Wait for equip to finish
        yield return new WaitForSeconds(weaponCurrent.GetEquipTime());

        weaponCurrent.Equip();
    }

    public void Throw(Vector3 targetPos)
    {
        if (weaponCurrent != null)
        {
            Vector3 velocity = (targetPos - transform.position).normalized;
            velocity.y += throwAngle;
            velocity = velocity.normalized;
            velocity *= throwSpeed;

            Vector3 angularVelocity = weaponCurrent.getObject().transform.right * throwAngularSpeed;

            weaponCurrent.Throw(velocity, angularVelocity);

            weaponCurrent = null;
            gunCurrent = null;
        }
    }

    public void ThrowToPlayer()
    {
        Throw(GameManager.instance.playerBodyPositions.playerCenter.position);
    }
}
