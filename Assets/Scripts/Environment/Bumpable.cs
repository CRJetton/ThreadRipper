using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bumpable : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float bumpForce;

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.isTrigger)
        {
            return;
        }

        if (_other.CompareTag("Player"))
        {
            rb.AddForce(_other.transform.forward * rb.mass * bumpForce, ForceMode.Impulse);
        }
    }
}
