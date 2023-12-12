using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bumpable : MonoBehaviour, IPhysics
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float playerBumpForce;
    [SerializeField] private float damageBumpForce;

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.isTrigger)
        {
            return;
        }

        if (_other.CompareTag("Player"))
        {
            rb.AddForce(_other.gameObject.transform.forward * playerBumpForce, ForceMode.Impulse);
        }
    }

    public void TakePhysics(Vector3 _hitForce)
    {
        rb.AddForce(_hitForce, ForceMode.Impulse);
    }
}
