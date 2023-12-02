using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GunController : MonoBehaviour, IGun
{
    [Header("General")]
    [SerializeField] GameObject bulletPrefab;

    [SerializeField] Transform barrelPos;

    [SerializeField, Range(0f, 5f)] float equipTime;
    [SerializeField, Range(0f, 10f)] float reloadTime;

    [SerializeField, Range(0f, 5f)] float timeBetweenShots;
    [SerializeField, Range(0f, 0.5f)] float queueShotTime;
    [SerializeField, Range(1, 30)] int bulletsPerShot;
    [SerializeField] bool fullAuto;
    [SerializeField] bool isPlayerGun;

    [Header("Scoping")]
    [SerializeField, Range(0f, 1f)] float scopeZoomFactor;
    [SerializeField, Range(0f, 5f)] float scopeInTime;
    [SerializeField, Range(0f, 5f)] float scopeOutTime;
    [SerializeField] Vector3 scopedOutPos;
    [SerializeField] Vector3 scopedInPos;
    [SerializeField] AnimationCurve scopeInCurve;
    [SerializeField] AnimationCurve scopeOutCurve;

    [Header("Scoped Recoil")]
    [SerializeField] AnimationCurve xScopedJumpPerShot;
    [SerializeField] AnimationCurve yScopedJumpPerShot;
    [SerializeField] AnimationCurve xScopedJumpRandomPerShot;
    [SerializeField] AnimationCurve yScopedJumpRandomPerShot;
    [SerializeField] AnimationCurve xScopedSpreadPerShot;
    [SerializeField] AnimationCurve yScopedSpreadPerShot;


    [Header("Hipfire Recoil")]
    [SerializeField] AnimationCurve xJumpPerShot;
    [SerializeField] AnimationCurve yJumpPerShot;
    [SerializeField] AnimationCurve xJumpRandomPerShot;
    [SerializeField] AnimationCurve yJumpRandomPerShot;
    [SerializeField] AnimationCurve xSpreadPerShot;
    [SerializeField] AnimationCurve ySpreadPerShot;

    [Header("Recoil Amounts")]
    [SerializeField, Range(0f, 5f)] float spreadRecoverTime;
    [SerializeField, Range(0f, 1f)] float spreadPerShot;
    [SerializeField, Range(0f, 2.5f)] float jumpRecoverTime;
    [SerializeField, Range(0f, 1f)] float dontRecoverTime;
    [SerializeField] AnimationCurve shotJumpRecovery;

    float recoilAmount;
    Vector2 amountShotJumped;

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
    Coroutine jumpRecovering;


    #region Initialization
    void Awake()
    {
        OnAmmoChange = new UnityEvent();
        OnShotJump = new UnityEvent<Vector3>();
        OnScoping = new UnityEvent<float, float>();
    }

    void Start()
    {
        ChangeAmmo(magAmmoCapacity, reserveAmmoCapacity);

        if (isPlayerGun)
            HUDManager.instance.reticleController.ExpandReticle(xSpreadPerShot.Evaluate(0));
    }
    #endregion

    #region Shooting
    public void AttackStarted()
    {
        if (!isEquipped)
            return;

        if (magAmmo > 0)
        {
            if (isReloading)
                CancelReload();

            Attack();

            if (fullAuto)
                repeatAttack = StartCoroutine(RepeatAttack());
        }
        else
        {
            Reload();
        }
    }

    public void AttackCanceled()
    {
        if (repeatAttack != null)
        {
            StopCoroutine(repeatAttack);
            repeatAttack = null;
        }

        StartJumpRecovery();
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

        AddRecoil(spreadPerShot);
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
                AttackCanceled();
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
            if (reloading != null)
                StopCoroutine(reloading);

            AttackCanceled();

            reloading = StartCoroutine(Reloading());
        }
    }

    void CancelReload()
    {
        if (reloading != null)
            StopCoroutine(reloading);

        isReloading = false;
        anim.SetBool("isReloading", isReloading);
    }

    IEnumerator Reloading()
    {
        isReloading = true;
        anim.SetBool("isReloading", isReloading);

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
        anim.SetBool("isReloading", isReloading);
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
        Vector2 shotJump = new Vector2(baseX, baseY);
        shotJump.x += Random.Range(-maxRandX, maxRandX);
        shotJump.y += Random.Range(-maxRandY, maxRandY);

        amountShotJumped += shotJump;

        OnShotJump.Invoke(shotJump);
    }

    void AddShotJumpDirectly(Vector3 amount)
    {
        OnShotJump.Invoke(amount);
    }

    UnityEvent<Vector3> OnShotJump;

    public void SubscribeOnShotJump(UnityAction<Vector3> action)
    {
        OnShotJump.AddListener(action);
    }


    Quaternion CalcShotRotation()
    {
        Vector3 finalRot = barrelPos.rotation.eulerAngles;

        float xMaxRand;
        float yMaxRand;

        if (!isScopedIn)
        {
            xMaxRand = xSpreadPerShot.Evaluate(recoilAmount);
            yMaxRand = ySpreadPerShot.Evaluate(recoilAmount);
        }
        else
        {
            xMaxRand = xScopedSpreadPerShot.Evaluate(recoilAmount);
            yMaxRand = yScopedSpreadPerShot.Evaluate(recoilAmount);
        }

        Vector3 randomOffset = new Vector3(Random.Range(-xMaxRand, xMaxRand), Random.Range(-yMaxRand, yMaxRand));

        finalRot += randomOffset;

        return Quaternion.Euler(finalRot);
    }


    IEnumerator Recoiling()
    {
        yield return new WaitForSeconds(dontRecoverTime);

        while (recoilAmount > 0)
        {
            recoilAmount -= (1 / spreadRecoverTime) * Time.fixedDeltaTime;
            recoilAmount = Mathf.Clamp01(recoilAmount);

            if (isPlayerGun)
                HUDManager.instance.reticleController.ExpandReticle(xSpreadPerShot.Evaluate(recoilAmount));

            yield return new WaitForFixedUpdate();
        }

        recoiling = null;
    }

    public void ContainerManuallyRotated(Vector2 amount)
    {
        if (Mathf.Abs(amountShotJumped.x + amount.x) < Mathf.Abs(amountShotJumped.x))
            amountShotJumped.x += amount.x;

        if (Mathf.Abs(amountShotJumped.y + amount.y) < Mathf.Abs(amountShotJumped.y))
            amountShotJumped.y += amount.y;
    }

    void StartJumpRecovery()
    {
        if (jumpRecovering != null)
            StopCoroutine(jumpRecovering);

        jumpRecovering = StartCoroutine(JumpRecovering());
    }


    IEnumerator JumpRecovering()
    {
        float startTime = Time.time;
        float percentDone = 0;

        Vector2 startJumpAmount = amountShotJumped;
        Vector2 nextJumpAmount;

        yield return new WaitForSeconds(dontRecoverTime);

        while (percentDone < 1)
        {
            percentDone = (Time.time - startTime) / jumpRecoverTime;

            nextJumpAmount = Vector2.Lerp(startJumpAmount, Vector2.zero, shotJumpRecovery.Evaluate(percentDone));

            AddShotJumpDirectly(nextJumpAmount - amountShotJumped);

            amountShotJumped = nextJumpAmount;

            yield return new WaitForNextFrameUnit();
        }

        jumpRecovering = null;
    }
    #endregion

    #region Aiming
    public void AimAt(Vector3 targetPosition)
    {
        transform.LookAt(targetPosition);
    }

    public void FocusStarted()
    {
        if (!isScopedIn)
        {
            if (scoping != null)
                StopCoroutine(scoping);

            scoping = StartCoroutine(DoScopeIn());
        }
    }

    public void FocusCanceled()
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
        anim.SetBool("isScopedIn", isScopedIn);

        if (isPlayerGun)
            HUDManager.instance.reticleController.gameObject.SetActive(false);

        OnScoping.Invoke(scopeZoomFactor, scopeInTime);

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
        anim.SetBool("isScopedIn", isScopedIn);

        if (isPlayerGun)
            HUDManager.instance.reticleController.gameObject.SetActive(true);

        OnScoping.Invoke(1, scopeOutTime);

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

    UnityEvent<float, float> OnScoping;

    public void SubscribeOnScoping(UnityAction<float, float> action)
    {
        OnScoping.AddListener(action);
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
