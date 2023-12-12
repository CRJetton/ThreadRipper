using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Explosive : MonoBehaviour, IDamageable
{
    [SerializeField] Renderer[] meshes;
    [SerializeField] Collider col;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float HP;
    [SerializeField] private float damage;
    [SerializeField] private float bumpForce;
    [SerializeField] private float damageRadius;


    public void TakeDamage(float _damage)
    {
        HP -= _damage;

        if (HP <= 0)
        {
            particles.Play();
            StartCoroutine(SelfDestruct());
            foreach(Renderer mesh in meshes)
            {
                mesh.enabled = false;
            }

            col.enabled = false;

            RaycastHit[] hits = Physics.SphereCastAll(transform.position, damageRadius, Vector3.up);
            foreach(RaycastHit hit in hits)
            {
                IDamageable hitDamageable = hit.collider.GetComponent<IDamageable>();
                IPhysics hitPhysical = hit.collider.GetComponent<IPhysics>();

                if (hitDamageable != null)
                {
                    hitDamageable.TakeDamage(damage);
                }

                if (hitPhysical != null)
                {
                    hitPhysical.TakePhysics((hit.transform.position - transform.position).normalized * bumpForce);
                }
            }
        }
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(particles.main.duration);
        Destroy(gameObject);
    }
}
