using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunController : MonoBehaviour, IGun
{
    [Header("General")]
    [SerializeField] GameObject bulletPrefab;

    [SerializeField] Transform barrelPos;

    [SerializeField] float equipTime;
    [SerializeField] float reloadTime;

    [SerializeField] float timeBetweenShots;
    [SerializeField] float queueShotTime;
    [SerializeField] int bulletsPerShot;
    [SerializeField] bool isSingleShot;

    [Header("Scoping")]
    [SerializeField] float scopeInTime;
    [SerializeField] float scopeOutTime;
    [SerializeField] Vector3 scopedOutPos;
    [SerializeField] Vector3 scopedInPos;
    [SerializeField] AnimationCurve scopeInCurve;
    [SerializeField] AnimationCurve scopeOutCurve;

    [Header("Scoped Recoil")]
    [SerializeField] AnimationCurve xScopedJumpPerShot;
    [SerializeField] AnimationCurve yScopedJumpPerShot;
    [SerializeField] AnimationCurve xScopedJumpRandomPerShot;
    [SerializeField] AnimationCurve yScopedJumpRandomPerShot;

    [Header("Hipfire Recoil")]
    [SerializeField] AnimationCurve xJumpPerShot;
    [SerializeField] AnimationCurve yJumpPerShot;
    [SerializeField] AnimationCurve xJumpRandomPerShot;
    [SerializeField] AnimationCurve yJumpRandomPerShot;
    [SerializeField] AnimationCurve xRecoilRandomPerShot;
    [SerializeField] AnimationCurve yRecoilRandomPerShot;

    [Header("Recoil Amounts")]
    [SerializeField] float recoilRecoverTime;
    [SerializeField, Range(0f, 1f)] float recoilPerShot;

    float recoilAmount;

    [Header("Ammo")]
    [SerializeField] int magAmmoCapacity;
    [SerializeField] int reserveAmmoCapacity;

    int magAmmo;
    int reserveAmmo;

    [Header("Animation")]
    [SerializeField] Animator anim;
    [SerializeField] AnimationClip[] shootAnims;

    bool isEquipped;
    bool isShooting;
    bool isShotQueued;
    bool isScopedIn;
    bool isReloading;

    Coroutine repeatAttack;
    Coroutine scoping;
    Coroutine reloading;
    Coroutine recoiling;


    void Awake()
    {
        OnAmmoChange = new UnityEvent();
        OnShotJump = new UnityEvent<Vector3>();
    }

    void Start()
    {
        ChangeAmmo(magAmmoCapacity, reserveAmmoCapacity);
    }


    #region Shooting
    public void StartAttack()
    {
        if (!isEquipped)
            return;

        if (magAmmo > 0)
        {
            if (isReloading)
                CancelReload();

            Attack();

            if (!isSingleShot)
                repeatAttack = StartCoroutine(RepeatAttack());
        }
        else
        {
            Reload();
        }
    }

    public void StopAttack()
    {
        if (repeatAttack != null)
        {
            StopCoroutine(repeatAttack);
            repeatAttack = null;
        }
    }

    void Attack()
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
        if (magAmmo <= 0 || !isEquipped)
            yield break;

        isShooting = true;

        for (int i = 0; i < bulletsPerShot; i++)
            Instantiate(bulletPrefab, barrelPos.position, CalcShotRotation());

        AddRecoil(recoilPerShot);
        ChangeAmmo(-1, 0);

        anim.Play(shootAnims[Random.Range(0, shootAnims.Length)].name);

        yield return new WaitForSeconds(timeBetweenShots);

        isShooting = false;

        if (isShotQueued)
        {
            isShotQueued = false;

            StartCoroutine(Shoot());
        }
    }

    Quaternion CalcShotRotation()
    {
        Vector3 finalRot = barrelPos.rotation.eulerAngles;

        if (!isScopedIn)
        {
            float xMaxRand = xRecoilRandomPerShot.Evaluate(recoilAmount);
            float yMaxRand = yRecoilRandomPerShot.Evaluate(recoilAmount);

            Vector3 randomOffset = new Vector3(Random.Range(-xMaxRand, xMaxRand), Random.Range(-yMaxRand, yMaxRand));

            finalRot += randomOffset;
        }

        return Quaternion.Euler(finalRot);
    }

    IEnumerator QueueShot()
    {
        isShotQueued = true;

        yield return new WaitForSeconds(queueShotTime);

        isShotQueued = false;
    }

    IEnumerator RepeatAttack()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(timeBetweenShots);

            if (magAmmo > 0 && isEquipped)
                Attack();
            else
            {
                StopAttack();
                yield break;
            }
        }
    }
    #endregion

    #region Ammo Control
    public void Reload()
    {
        if (!isReloading && magAmmo < magAmmoCapacity && reserveAmmo > 0)
        {
            reloading = StartCoroutine(Reloading());
        }
    }

    void CancelReload()
    {
        if (reloading != null)
            StopCoroutine(reloading);

        isReloading = false;
    }

    IEnumerator Reloading()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        int maxReloadAmount = magAmmoCapacity - magAmmo;

        if (reserveAmmo >= maxReloadAmount)
        {
            ChangeAmmo(maxReloadAmount, -maxReloadAmount);
        }
        else
        {
            ChangeAmmo(reserveAmmo, -reserveAmmo);
        }

        isReloading = false;
    }

    void ChangeAmmo(int magChange, int reserveChange)
    {
        magAmmo += magChange;
        reserveAmmo += reserveChange;

        OnAmmoChange.Invoke();
    }

    UnityEvent OnAmmoChange;

    public void SubscribeOnAmmoChange(UnityAction action)
    {
        OnAmmoChange.AddListener(action);
    }
    #endregion

    #region Recoil
    void AddRecoil(float amount)
    {
        recoilAmount += amount;
        recoilAmount = Mathf.Clamp01(recoilAmount);

        if (recoiling == null)
            recoiling = StartCoroutine(Recoiling());

        if (isScopedIn)
            AddShotJump(xScopedJumpPerShot.Evaluate(recoilAmount), yScopedJumpPerShot.Evaluate(recoilAmount),
                xScopedJumpRandomPerShot.Evaluate(recoilAmount), yScopedJumpRandomPerShot.Evaluate(recoilAmount));
        else
            AddShotJump(xJumpPerShot.Evaluate(recoilAmount), yJumpPerShot.Evaluate(recoilAmount),
                xJumpRandomPerShot.Evaluate(recoilAmount), yJumpRandomPerShot.Evaluate(recoilAmount));
    }

    void AddShotJump(float baseX, float baseY, float maxRandX, float maxRandY)
    {
        Vector3 shotJump = new Vector3(baseX, baseY);
        shotJump.x += Random.Range(-maxRandX, maxRandX);
        shotJump.y += Random.Range(-maxRandY, maxRandY);

        OnShotJump.Invoke(shotJump);
    }

    IEnumerator Recoiling()
    {
        while (recoilAmount > 0)
        {
            recoilAmount -= (1 / recoilRecoverTime) * Time.fixedDeltaTime;
            recoilAmount = Mathf.Clamp01(recoilAmount);

            yield return new WaitForFixedUpdate();
        }

        recoiling = null;
    }

    UnityEvent<Vector3> OnShotJump;

    public void SubscribeOnShotJump(UnityAction<Vector3> action)
    {
        OnShotJump.AddListener(action);
    }
    #endregion

    #region Aiming
    public void AimAt(Vector3 targetPosition)
    {
        transform.LookAt(targetPosition);
    }

    public void StartFocus()
    {
        if (!isScopedIn)
        {
            if (scoping != null)
                StopCoroutine(scoping);

            scoping = StartCoroutine(DoScopeIn());
        }
    }

    public void StopFocus()
    {
        if (isScopedIn)
        {
            if (scoping != null)
                StopCoroutine(scoping);

            scoping = StartCoroutine(DoScopeOut());
        }
    }

    IEnumerator DoScopeIn()
    {
        float startTime = Time.time;
        float percentDone = 0;

        Vector3 startPos = transform.localPosition;

        isScopedIn = true;

        for (; ; )
        {
            percentDone = (Time.time - startTime) / scopeInTime;

            transform.localPosition = Vector3.Lerp(startPos, scopedInPos, scopeInCurve.Evaluate(percentDone));

            if (percentDone >= 1)
            {
                transform.localPosition = scopedInPos;
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator DoScopeOut()
    {
        float startTime = Time.time;
        float percentDone = 0;

        Vector3 startPos = transform.localPosition;

        isScopedIn = false;

        for (; ; )
        {
            percentDone = (Time.time - startTime) / scopeOutTime;

            transform.localPosition = Vector3.Lerp(startPos, scopedOutPos, scopeOutCurve.Evaluate(percentDone));

            if (percentDone >= 1)
            {
                transform.localPosition = scopedOutPos;
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }
    #endregion

    #region Equipping
    public void Equip()
    {
        isEquipped = true;
    }
    #endregion

    #region Getters
    public float GetEquipTime()
    {
        return equipTime;
    }

    public int GetMagAmmo()
    {
        return magAmmo;
    }

    public int GetReserveAmmo()
    {
        return reserveAmmo;
    }
    #endregion
}
