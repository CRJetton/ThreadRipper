using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject playerItemPrefab;
    [SerializeField] GameObject enemyItemPrefab;


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

    public void Throw()
    {

    }

    #region Getters
    public GameObject GetPlayerItemPrefab() { return playerItemPrefab; }

    public GameObject GetEnemyItemPrefab() { return enemyItemPrefab; }
    #endregion
}
