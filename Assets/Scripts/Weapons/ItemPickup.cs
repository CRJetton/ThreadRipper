using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject playerItemPrefab;
    [SerializeField] GameObject enemyItemPrefab;

    [SerializeField] Rigidbody rb;

    [SerializeField] float thrownDamage;

    bool isThrown;

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

    public void Throw(Vector3 velocity)
    {
        isThrown = true;

        rb.velocity = velocity;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || isThrown)
            return;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(thrownDamage);

            if (other.CompareTag("Enemy"))
            {
                // force enemy gun throw
            }
        }
    }

    #region Getters
    public GameObject GetPlayerItemPrefab() { return playerItemPrefab; }

    public GameObject GetEnemyItemPrefab() { return enemyItemPrefab; }
    #endregion
}
