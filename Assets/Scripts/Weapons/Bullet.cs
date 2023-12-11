using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    string noHitTag;
    float damage;
    float speed;
    float destroyTime;

    [SerializeField] Rigidbody rb;

    bool isPlayerRound;


    public void Initialize(string _noHitTag, float _damage, float _speed, float _destroyTime)
    {
        noHitTag = _noHitTag;
        damage = _damage;
        speed = _speed;
        destroyTime = _destroyTime;

        rb.velocity = transform.forward * speed;

        Destroy(gameObject, destroyTime);

        isPlayerRound = noHitTag == "Player";
    }


    private void FixedUpdate()
    {
        HitControl();
    }


    void HitControl()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, speed * Time.fixedDeltaTime))
        {
            Hit(hit.collider, hit);
        }
    }


    void Hit(Collider other, RaycastHit hit)
    {
        if (other.isTrigger || other.CompareTag(noHitTag))
            return;

        transform.position = hit.point;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null)
        {
            if (isPlayerRound && other.CompareTag("Enemy"))
                HUDManager.instance.ShowHitMarker();

            damageable.TakeDamage(damage);
        }
        else
        {
            DecalManager.instance.CreateBulletHole(hit.point, hit.normal);
        }

        Destroy(gameObject);
    }
}
