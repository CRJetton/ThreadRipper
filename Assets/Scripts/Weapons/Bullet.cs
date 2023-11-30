using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] string noHitTag;

    [SerializeField] int damage;
    [SerializeField] float speed;
    [SerializeField] float destroyTime;

    [SerializeField] Rigidbody rb;



    private void Start()
    {
        rb.velocity = transform.forward * speed;

        Destroy(gameObject, destroyTime);
    }


    private void FixedUpdate()
    {
        HitControl();
    }


    void HitControl()
    {
        RaycastHit hit;

        if (Physics.Raycast(rb.position, transform.forward, out hit, speed * Time.fixedDeltaTime))
        {
            Hit(hit.collider, hit.point);
        }
    }


    void Hit(Collider other, Vector3 point)
    {
        if (other.isTrigger || other.CompareTag(noHitTag))
            return;

        transform.position = point;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
            damageable.TakeDamage(damage);

        Destroy(gameObject);
    }
}
