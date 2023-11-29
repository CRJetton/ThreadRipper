using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour, IGun
{
    [SerializeField] GameObject bulletPrefab;

    [SerializeField] Transform barrelPos;

    [SerializeField] float timeBetweenShots;
    [SerializeField] float queueShotTime;

    [SerializeField] float scopeInTime;
    [SerializeField] float scopeOutTime;

    [SerializeField] int magAmmoCapacity;
    [SerializeField] int reserveAmmoCapacity;

    int magAmmo;
    int reserveAmmo;

    bool isShooting;
    bool isShotQueued;

    Coroutine repeatAttack;



    #region Shooting

    public void StartAttack()
    {
        Attack();

        repeatAttack = StartCoroutine(RepeatAttack());
    }

    public void StopAttack()
    {
        if (repeatAttack != null)
        {
            StopCoroutine(repeatAttack);
            repeatAttack = null;
        }
    }

    public void Attack()
    {
        if (!isShooting)
        {
            StartCoroutine(Shoot());
        }
        else if (!isShotQueued)
        {
            StartCoroutine(QueueShot());
        }
    }

    IEnumerator Shoot()
    {
        isShooting = true;

        Instantiate(bulletPrefab, barrelPos.position, barrelPos.rotation);

        yield return new WaitForSeconds(timeBetweenShots);

        isShooting = false;

        if (isShotQueued)
        {
            isShotQueued = false;

            StartCoroutine(Shoot());
        }
    }

    IEnumerator QueueShot()
    {
        isShotQueued = true;

        yield return new WaitForSeconds(queueShotTime);

        isShotQueued = false;
    }

    IEnumerator RepeatAttack()
    {
        for(; ; )
        {
            yield return new WaitForSeconds(timeBetweenShots);

            Attack();
        }
    }
    #endregion

    #region Reloading
    public void Reload()
    {
    }
    #endregion

    #region Aiming
    public void AimAt(Vector3 targetPosition)
    {
        transform.LookAt(targetPosition);
    }

    public void ScopeIn()
    {
    }

    public void ScopeOut()
    {
    }
    #endregion
}
