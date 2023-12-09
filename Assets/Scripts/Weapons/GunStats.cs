using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GunStats : WeaponStats
{

    [Header("General")]
    public string noHitTag;
    [Range(0f, 5f)] public float equipTime;
    [Range(0f, 10f)] public float reloadTime;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float damage;
    public float bulletSpeed;
    public float bulletDestroyTime;

    [Range(0f, 5f)] public float timeBetweenShots;
    [Range(0f, 0.5f)] public float queueShotTime;
    [Range(1, 30)] public int bulletsPerShot;
    public bool fullAuto;

    [Header("Ammo")]
    public int magAmmoCapacity;
    public int reserveAmmoCapacity;

    [Header("Scoping")]
    [Range(0f, 1f)] public float scopeZoomFactor;
    [Range(0f, 5f)] public float scopeInTime;
    [Range(0f, 5f)] public float scopeOutTime;
    public Vector3 scopedOutPos;
    public Vector3 scopedInPos;
    public AnimationCurve scopeInCurve;
    public AnimationCurve scopeOutCurve;

    [Header("Scoped Recoil")]
    public AnimationCurve xScopedJumpPerShot;
    public AnimationCurve yScopedJumpPerShot;
    public AnimationCurve xScopedJumpRandomPerShot;
    public AnimationCurve yScopedJumpRandomPerShot;
    public AnimationCurve xScopedSpreadPerShot;
    public AnimationCurve yScopedSpreadPerShot;


    [Header("Hipfire Recoil")]
    public AnimationCurve xJumpPerShot;
    public AnimationCurve yJumpPerShot;
    public AnimationCurve xJumpRandomPerShot;
    public AnimationCurve yJumpRandomPerShot;
    public AnimationCurve xSpreadPerShot;
    public AnimationCurve ySpreadPerShot;

    [Header("Recoil Amounts")]
    [Range(0f, 5f)] public float spreadRecoverTime;
    [Range(0f, 1f)] public float spreadPerShot;
    [Range(0f, 2.5f)] public float jumpRecoverTime;
    [Range(0f, 1f)] public float dontRecoverTime;
    public AnimationCurve shotJumpRecovery;

    [Header("Audio")]
    public AudioClip[] shootSounds;
    [Range(0, 1)] public float shootVolume;
    public AudioClip reloadSound;
    [Range(0, 1)] public float reloadVolume;
}
