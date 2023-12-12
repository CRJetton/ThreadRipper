using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GunController : MonoBehaviour, IGun
{
    [Header("Components")]
    [SerializeField] Transform barrelPos;
    [SerializeField] GameObject model;
    [SerializeField] Animator anim;
    [SerializeField] GameObject scope;
    [SerializeField] AudioSource audioSource;

    [Header("Animation")]
    [SerializeField] AnimationClip[] shootAnims;

    [SerializeField] GunStats gun;

    float recoilAmount;
    Vector2 amountShotJumped;

    int magAmmo;
    int reserveAmmo;

    bool isPlayerGun;
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

    bool ammoInitialized;
    bool animateGun;


    #region Initialization
    void Awake()
    {
        OnAmmoChange = new UnityEvent();
        OnShotJump = new UnityEvent<Vector3>();
        OnScoping = new UnityEvent<float, float>();
    }

    void Start()
    {
        InitializeAmmo(gun.magAmmoCapacity, gun.reserveAmmoCapacity);

        isPlayerGun = gun.noHitTag == "Player";

        if (isPlayerGun)
        {
            animateGun = true;

            HUDManager.instance.reticleController.ExpandReticle(gun.xSpreadPerShot.Evaluate(0));

            //sets the gun's layer to 'hands'
            model.layer = 6;

            if (scope != null)
                Instantiate(scope, model.transform);
        }
    }

    public void SetStats(WeaponStats stats)
    {
        gun = stats as GunStats;
    }

    public void InitializeAmmo(int _magAmmo, int _reserveAmmo)
    {
        if (!ammoInitialized)
        {
            ChangeAmmo(_magAmmo, _reserveAmmo);
            ammoInitialized = true;
        }
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

            if (gun.fullAuto)
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

        for (int i = 0; i < gun.bulletsPerShot; i++)
            CreateBullet();

        AddRecoil(gun.spreadPerShot);
        ChangeAmmo(-1, 0);

        if (animateGun)
            anim.Play(shootAnims[Random.Range(0, shootAnims.Length)].name);

        PlaySound(gun.shootSounds[Random.Range(0, gun.shootSounds.Length - 1)], gun.shootVolume, gun.minShootPitch, gun.maxShootPitch);

        Instantiate(gun.muzzleFlash, barrelPos.position, barrelPos.rotation);

        StopJumpRecovery();
        StartJumpRecovery();

        yield return new WaitForSeconds(gun.timeBetweenShots);

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

        yield return new WaitForSeconds(gun.queueShotTime);

        isShotQueued = false;
    }

    IEnumerator RepeatAttack()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(gun.timeBetweenShots);

            if (magAmmo > 0 && isEquipped)
                Attack();
            else
            {
                AttackCanceled();
                yield break;
            }
        }
    }

    void CreateBullet()
    {
        GameObject bullet = Instantiate(gun.bulletPrefab, barrelPos.position, CalcShotRotation());
        bullet.GetComponent<Bullet>().Initialize(gun.noHitTag, gun.damage, gun.hitForce, gun.bulletSpeed, gun.bulletDestroyTime,
            gun.hitWallEffect, gun.hitEntityEffect);
    }
    #endregion

    #region Ammo Control
    public void Reload()
    {
        if (!isReloading && magAmmo < gun.magAmmoCapacity && reserveAmmo > 0)
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

        if (animateGun)
            anim.SetBool("isReloading", isReloading);
    }

    IEnumerator Reloading()
    {
        isReloading = true;
        if (animateGun)
            anim.SetBool("isReloading", isReloading);

        PlaySound(gun.reloadSound, gun.reloadVolume, gun.minReloadPitch, gun.maxReloadPitch);

        yield return new WaitForSeconds(gun.reloadTime);

        if (isPlayerGun)
        {
            int maxReloadAmount = gun.magAmmoCapacity - magAmmo;

            if (reserveAmmo >= maxReloadAmount)
            {
                ChangeAmmo(maxReloadAmount, -maxReloadAmount);
            }
            else
            {
                ChangeAmmo(reserveAmmo, -reserveAmmo);
            }
        }
        else
        {
            ChangeAmmo(gun.magAmmoCapacity, 0);
        }

        isReloading = false;

        if (animateGun)
            anim.SetBool("isReloading", isReloading);
    }

    void ChangeAmmo(int magChange, int reserveChange)
    {
        magAmmo += magChange;
        reserveAmmo += reserveChange;

        magAmmo = Mathf.Clamp(magAmmo, 0, gun.magAmmoCapacity);
        reserveAmmo = Mathf.Clamp(reserveAmmo, 0, gun.reserveAmmoCapacity);

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
            recoiling = StartCoroutine(RecoilRecovery());

        if (isScopedIn)
            AddShotJump(gun.xScopedJumpPerShot.Evaluate(recoilAmount), gun.yScopedJumpPerShot.Evaluate(recoilAmount),
                gun.xScopedJumpRandomPerShot.Evaluate(recoilAmount), gun.yScopedJumpRandomPerShot.Evaluate(recoilAmount));
        else
            AddShotJump(gun.xJumpPerShot.Evaluate(recoilAmount), gun.yJumpPerShot.Evaluate(recoilAmount),
                gun.xJumpRandomPerShot.Evaluate(recoilAmount), gun.yJumpRandomPerShot.Evaluate(recoilAmount));

        if (isPlayerGun)
            HUDManager.instance.reticleController.ExpandReticle(gun.xSpreadPerShot.Evaluate(recoilAmount));
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
            xMaxRand = gun.xSpreadPerShot.Evaluate(recoilAmount);
            yMaxRand = gun.ySpreadPerShot.Evaluate(recoilAmount);
        }
        else
        {
            xMaxRand = gun.xScopedSpreadPerShot.Evaluate(recoilAmount);
            yMaxRand = gun.yScopedSpreadPerShot.Evaluate(recoilAmount);
        }

        Vector3 randomOffset = new Vector3(Random.Range(-xMaxRand, xMaxRand), Random.Range(-yMaxRand, yMaxRand));

        finalRot += randomOffset;

        return Quaternion.Euler(finalRot);
    }


    IEnumerator RecoilRecovery()
    {
        yield return new WaitForSeconds(gun.dontRecoverTime);

        while (recoilAmount > 0)
        {
            recoilAmount -= (1 / gun.spreadRecoverTime) * Time.fixedDeltaTime;
            recoilAmount = Mathf.Clamp01(recoilAmount);

            if (isPlayerGun)
                HUDManager.instance.reticleController.ExpandReticle(gun.xSpreadPerShot.Evaluate(recoilAmount));

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

    void StopJumpRecovery()
    {
        if (jumpRecovering != null)
        {
            StopCoroutine(jumpRecovering);

            jumpRecovering = null;
        }
    }


    IEnumerator JumpRecovering()
    {
        float startTime = Time.time;
        float percentDone = 0;

        Vector2 startJumpAmount = amountShotJumped;
        Vector2 nextJumpAmount;

        yield return new WaitForSeconds(gun.dontRecoverTime);

        while (percentDone < 1)
        {
            percentDone = (Time.time - startTime) / gun.jumpRecoverTime;

            nextJumpAmount = Vector2.Lerp(startJumpAmount, Vector2.zero, gun.shotJumpRecovery.Evaluate(percentDone));

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

        if (isPlayerGun)
            HUDManager.instance.HideReticle();

        OnScoping.Invoke(gun.scopeZoomFactor, gun.scopeInTime);

        for (; ; )
        {
            percentDone = (Time.time - startTime) / gun.scopeInTime;

            transform.localPosition = Vector3.Lerp(startPos, gun.scopedInPos, gun.scopeInCurve.Evaluate(percentDone));

            if (percentDone >= 1)
            {
                transform.localPosition = gun.scopedInPos;
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

        if (isPlayerGun)
            HUDManager.instance.ShowReticle();

        OnScoping.Invoke(1, gun.scopeOutTime);

        for (; ; )
        {
            percentDone = (Time.time - startTime) / gun.scopeOutTime;

            transform.localPosition = Vector3.Lerp(startPos, gun.scopedOutPos, gun.scopeOutCurve.Evaluate(percentDone));

            if (percentDone >= 1)
            {
                transform.localPosition = gun.scopedOutPos;
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

    #region Equipping, Dropping, Throwing
    public void Equip()
    {
        isEquipped = true;
    }

    public void Drop()
    {
        transform.parent = null;

        GameObject spawnedItem = Instantiate(gun.itemPickupPrefab, transform.position, transform.rotation);
        WeaponPickup itemPickup = spawnedItem.GetComponent<WeaponPickup>();
        itemPickup.SetSaveValue1(magAmmo);
        itemPickup.SetSaveValue2(reserveAmmo);
        itemPickup.Drop();

        Destroy(gameObject);
    }

    public void Throw(Vector3 velocity, Vector3 angularVelocity)
    {
        transform.parent = null;

        GameObject spawnedItem = Instantiate(gun.itemPickupPrefab, transform.position, transform.rotation);
        WeaponPickup itemPickup = spawnedItem.GetComponent<WeaponPickup>();
        itemPickup.SetSaveValue1(magAmmo);
        itemPickup.SetSaveValue2(reserveAmmo);
        itemPickup.Throw(velocity, angularVelocity, gun.noHitTag);

        Destroy(gameObject);
    }
    #endregion

    #region Audio
    public void PlaySound(AudioClip clip, float volume, float minPitch, float maxPitch)
    {
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, volume);
    }
    #endregion

    #region Getters and Setters
    public float GetEquipTime()
    {
        return gun.equipTime;
    }

    public int GetMagAmmo()
    {
        return magAmmo;
    }

    public int GetReserveAmmo()
    {
        return reserveAmmo;
    }

    public GameObject GetObject()
    {
        return gameObject;
    }

    public void SetMagAmmo(int ammo)
    {
        magAmmo = ammo;
    }

    public void SetReserveAmmo(int ammo)
    {
        reserveAmmo = ammo;
    }

    public int GetMagAmmoCapacity()
    {
        return gun.magAmmoCapacity;
    }

    public int GetReserveAmmoCapacity()
    {
        return gun.reserveAmmoCapacity;
    }
    #endregion
}
