using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour, IInteractable
{
    [SerializeField] string itemName;

    [SerializeField] WeaponStats playerWeapon;
    [SerializeField] WeaponStats enemyWeapon;
    [SerializeField] Collider physicsCol;

    [SerializeField] Rigidbody rb;

    [SerializeField] float thrownDamage;
    [SerializeField] float dropAngularSpeed;

    [SerializeField] int currentAmmo;
    [SerializeField] int reserveAmmo;


    bool isThrown;
    string noHitTag;

    public void Interact(IInteractionController interactionController)
    {
        interactionController.PickUpWeapon(this);
    }

    public string GetName()
    {
        return itemName;
    }

    public void PickedUp()
    {
        Destroy(gameObject);
    }

    public void Drop()
    {
        rb.angularVelocity = transform.forward * Random.Range(-dropAngularSpeed, dropAngularSpeed);
    }

    #region Throwing
    public void Throw(Vector3 velocity, Vector3 angularVelocity, string _noHitTag)
    {
        isThrown = true;
        physicsCol.enabled = false;

        noHitTag = _noHitTag;

        rb.velocity = velocity;
        rb.angularVelocity = angularVelocity;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || !isThrown || other.CompareTag(noHitTag))
            return;

        isThrown = false;
        physicsCol.enabled = true;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(thrownDamage);

            if (other.CompareTag("Enemy"))
            {
                IEnemyCombat enemyCombat = other.GetComponent<IEnemyCombat>();

                enemyCombat.ThrowToPlayer();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger || !isThrown)
            return;

        if (other.CompareTag(noHitTag))
            physicsCol.enabled = true;
    }
    #endregion

    #region Getters and Setters
    public WeaponStats GetPlayerWeapon() { return playerWeapon; }

    public WeaponStats GetEnemyWeapon() { return enemyWeapon; }

    public void SetSaveValue1(int value) { currentAmmo = value; }
    public void SetSaveValue2(int value) { reserveAmmo = value; }

    public int GetSaveValue1() { return currentAmmo; }
    public int GetSaveValue2() { return reserveAmmo; }
    #endregion
}
