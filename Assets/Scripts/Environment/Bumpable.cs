using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bumpable : MonoBehaviour, IDamageable
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float bumpForce;
    private Vector3 impactDirection;

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.isTrigger)
        {
            return;
        }

        impactDirection = _other.gameObject.transform.forward;

        if (_other.CompareTag("Player"))
        {
            rb.AddForce(impactDirection * bumpForce, ForceMode.Impulse);
        }
    }

    public void TakeDamage(float _damage)
    {
        rb.AddForce(impactDirection * bumpForce, ForceMode.Impulse);
    }
}
