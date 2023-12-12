using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BossCombat : MonoBehaviour
{
    [Header("------ Boss Gun Array ------")]
    [SerializeField] private GameObject[] shotgunCannons;
    [SerializeField] private GameObject[] sniperRifles;
    [SerializeField] private GameObject[] automaticRifles;

    [Header("------ Boss Hardpoints ------")]
    [SerializeField] private Transform[] shotgunHardpoints;
    [SerializeField] private Transform[] sniperHardpoints;
    [SerializeField] private Transform[] rifleHardpoints;

    [SerializeField] private Animator animator;

    private List<IWeapon> equippedShotguns = new List<IWeapon>();
    private List<IWeapon> equippedSnipers = new List<IWeapon>();
    private List<IWeapon> equippedRifles = new List<IWeapon>();

    private float HP;
    private float maxHP;
    private int currentPhase = 1;
    private float phase2Threshold = 0.66f;
    private float phase3Threshold = 0.33f;

    #region initialization
    // At start we are equiping the array of weapons to the boss
    private void Start()
    {
        StartCoroutine(EquipAllWeapons());
    }

    // This initializes the hp of the boss to use as reference to change the bosses phase state
    public void InitializeMaxHP(float maxHealthPoints)
    {
        maxHP = maxHealthPoints;
    }
    #endregion

    #region phase state
    // here the boss changes states depending on his health values
    public void ChangePhase(float hp)
    {
        HP = hp;

        int newPhase = CalculateCurrentPhase();
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            GetComponent<BossAI>().UpdateSpeed(currentPhase);
        }
    }

    // here we are determining the phase based on the max hp snapshoted at initialization and calculated against our phase threshold
    private int CalculateCurrentPhase()
    {
        if (HP <= phase3Threshold * maxHP)
        {
            return 3;
        }
        else if (HP <= phase2Threshold * maxHP)
        {
            return 2;
        }
        return 1;
    }
    #endregion

    #region weapon behavior
    // here we are equping each of the weapon arrays to our model
    private IEnumerator EquipAllWeapons()
    {
        yield return StartCoroutine(EquipWeaponArray(shotgunCannons, equippedShotguns, shotgunHardpoints));
        yield return StartCoroutine(EquipWeaponArray(sniperRifles, equippedSnipers, sniperHardpoints));
        yield return StartCoroutine(EquipWeaponArray(automaticRifles, equippedRifles, rifleHardpoints));
    }

    // this iterates through the weapon array to equip each weapon
    private IEnumerator EquipWeaponArray(GameObject[] weaponPrefabs, List<IWeapon> weaponList, Transform[] hardpoints)
    {
        for (int index = 0; index < weaponPrefabs.Length; index++)
        {
            IWeapon weaponScript = weaponPrefabs[index].GetComponent<IWeapon>();

            if (weaponScript != null)
            {
                if (weaponScript is IGun gun)
                {
                    gun.InitializeAmmo(gun.GetMagAmmoCapacity(), Random.Range(0, gun.GetMagAmmoCapacity()));
                    yield return new WaitForSeconds(gun.GetEquipTime());
                    gun.Equip();
                }
                weaponList.Add(weaponScript);
            }
        }
    }
    #endregion

    #region combat
    // Here we are aiming at the players world position
    public void AimAtPlayer(Vector3 worldPosition)
    {
        AimWeaponListAt(equippedShotguns, worldPosition);
        AimWeaponListAt(equippedRifles, worldPosition);
        AimWeaponListAt(equippedSnipers, worldPosition);
    }

    private void AimWeaponListAt(List<IWeapon> weapons, Vector3 worldPosition)
    {
        foreach (var weapon in weapons)
        {
            weapon.AimAt(worldPosition);
        }
    }

    // this begins the firing routine
    public void FireWeapons()
    {
        
        FireWeaponList(equippedShotguns);
        animator.SetTrigger("Shoot Cannons");
        // animator.ResetTrigger("Shoot Cannons");

        if (currentPhase >= 2)
        {
            FireWeaponList(equippedRifles);
        }

        if (currentPhase == 3)
        {
            FireWeaponList(equippedSnipers);
        }
    }

    private void FireWeaponList(List<IWeapon> weapons)
    {
        foreach (var weapon in weapons)
        {
            weapon.AttackStarted();
        }
    }
}
#endregion