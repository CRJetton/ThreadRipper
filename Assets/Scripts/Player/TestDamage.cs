using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDamage : MonoBehaviour
{
    private void OnTriggerEnter(Collider _other)
    {
        IDamageable dam = _other.GetComponent<IDamageable>();
        if (dam != null) dam.TakeDamage(1);
    }
}
