using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyForDrop : MonoBehaviour, IDamageable
{
    [SerializeField] Renderer mesh;
    [SerializeField] Collider col;
    [SerializeField] private float HP;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] GameObject drop;
    [SerializeField] private float destroyToDropTime;

    public void TakeDamage(float _damage)
    {
        HP -= _damage;

        if (HP <= 0)
        {
            StartCoroutine(Drop());
            particles.Play();
            mesh.enabled = false;
            col.enabled = false;
        }
    }

    private IEnumerator Drop()
    {
        yield return new WaitForSeconds(destroyToDropTime);
        drop.SetActive(true);
    }
}
