using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject playerItemPrefab;
    [SerializeField] GameObject enemyItemPrefab;
    [SerializeField] Collider physicsCol;

    [SerializeField] Rigidbody rb;

    [SerializeField] float thrownDamage;

    [SerializeField] int saveValue1;
    [SerializeField] int saveValue2;


    bool isThrown;
    string noHitTag;

    public void Interact(IInteractionController interactionController)
    {
        interactionController.PickUpItem(this);
    }
    public float GetSquareDistance(Vector3 origin)
    {
        return Vector3.Magnitude(origin - transform.position);
    }

    public void PickedUp()
    {
        Destroy(gameObject);
    }

    public void Drop()
    {

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
    #endregion

    #region Getters and Setters
    public GameObject GetPlayerItemPrefab() { return playerItemPrefab; }

    public GameObject GetEnemyItemPrefab() { return enemyItemPrefab; }

    public void SetSaveValue1(int value) { saveValue1 = value; }
    public void SetSaveValue2(int value) { saveValue2 = value; }

    public int GetSaveValue1() { return saveValue1; }
    public int GetSaveValue2() { return saveValue2; }
    #endregion
}
