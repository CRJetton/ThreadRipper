using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    BulletStats bulletStats;

    [SerializeField] Rigidbody rb;

    bool isPlayerRound;


    public void Initialize(BulletStats stats)
    {
        bulletStats = stats;

        rb.velocity = transform.forward * bulletStats.speed;

        Destroy(gameObject, bulletStats.destroyTime);

        isPlayerRound = bulletStats.noHitTag == "Player";
    }


    private void FixedUpdate()
    {
        HitControl();
    }


    void HitControl()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, bulletStats.speed * Time.fixedDeltaTime))
        {
            Hit(hit.collider, hit);
        }
    }


    void Hit(Collider other, RaycastHit hit)
    {
        if (other.isTrigger || other.CompareTag(bulletStats.noHitTag))
            return;

        transform.position = hit.point;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
        {
            if (isPlayerRound && other.CompareTag("Enemy"))
            { /* hit marker call */ }

            damageable.TakeDamage(bulletStats.damage);
        }
        else
        {
            DecalManager.instance.CreateBulletHole(hit.point, hit.normal);
        }

        Destroy(gameObject);
    }
}
