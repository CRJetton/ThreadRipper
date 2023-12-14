using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour, IEnemyCombat
{
    [Header("Components")]
    [SerializeField] BaseAI baseAI;

    [Header("Combat")]
    [SerializeField] Transform weaponContainer;
    [SerializeField] GameObject startingWeapon;

    [SerializeField] float aimAtSmoothing;
    Vector3 aimPos;
    Vector3 lastAimPos;
    bool isLastAimInitialized;

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
        baseAI.SubscribeOnDie(Die);
    }


    public void AimAt(Vector3 worldPosition)
    {
        if (!isLastAimInitialized)
        {
            lastAimPos = weaponCurrent.GetObject().transform.position + weaponCurrent.GetObject().transform.forward;
            isLastAimInitialized = true;
        }

        if (weaponCurrent != null)
        {
            aimPos = Vector3.Lerp(lastAimPos, worldPosition, aimAtSmoothing * Time.deltaTime);
            weaponCurrent.AimAt(aimPos);

            lastAimPos = aimPos;
        }
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
        GameObject weaponClone = Instantiate(pickup.GetEnemyWeapon().weaponPrefab, weaponContainer);

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

        if (weaponCurrent != null)
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

            Vector3 angularVelocity = weaponCurrent.GetObject().transform.right * throwAngularSpeed;

            weaponCurrent.Throw(velocity, angularVelocity);

            weaponCurrent = null;
            gunCurrent = null;
        }
    }

    public void ThrowToPlayer()
    {
        if (gunCurrent != null)
        {
            gunCurrent.SetMagAmmo(Mathf.Clamp(gunCurrent.GetMagAmmo(), gunCurrent.GetMagAmmoCapacity() / 2, gunCurrent.GetMagAmmoCapacity()));
        }

        Throw(GameManager.instance.playerBodyPositions.GetPlayerCenter());
    }


    public void Die()
    {
        if (weaponCurrent != null)
        {
            if (gunCurrent != null)
            {
                gunCurrent.SetMagAmmo(Mathf.Clamp(gunCurrent.GetMagAmmo(), gunCurrent.GetMagAmmoCapacity() / 2, gunCurrent.GetMagAmmoCapacity()));
            }


            weaponCurrent.Drop();
        }
    }
}
