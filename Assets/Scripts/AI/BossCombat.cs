using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/*
 * Author: Tristan Sherzer
 * 
 * Contact info: tsherzer@gmail.com
 *               239-370-1229
 *               12/14/23 P&F2
 */

// Controls the combat actions of the boss, including weapon management and attack phases
public class BossCombat : MonoBehaviour
{
    // Arrays of game objects to hold weapon prefabs
    [Header("------ Boss Gun Array ------")]
    [SerializeField] private GameObject[] shotgunCannons;
    [SerializeField] private GameObject[] sniperRifles;
    [SerializeField] private GameObject[] automaticRifles;

    // Hardpoints for mounting of the weapons on the boss model
    [Header("------ Boss Hardpoints ------")]
    [SerializeField] private Transform[] shotgunHardpoints;
    [SerializeField] private Transform[] sniperHardpoints;
    [SerializeField] private Transform[] rifleHardpoints;

    // Animator for controlling boss animations
    [SerializeField] private Animator animator;

    // Lists to keep track of equipped weapon prefabs
    private List<IWeapon> equippedShotguns = new List<IWeapon>();
    private List<IWeapon> equippedSnipers = new List<IWeapon>();
    private List<IWeapon> equippedRifles = new List<IWeapon>();

    // Variables for tracking boss health and combat phases
    private float HP;
    private float maxHP;
    private int currentPhase = 1;
    private float phase2Threshold = 0.66f;
    private float phase3Threshold = 0.33f;

    // Initialization of boss's state of combat at the start of the game
    #region initialization
    // Equips weapons at BossCombat start
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

    // The bosses phase logic for changing between states
    #region phase state
    // Boss changes states depending on his health values
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

    // Determines the phase based on the max hp snapshoted at initialization and calculated against our phase threshold
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

    // Weapon logic for equiping and handling weapon arrays
    #region weapon behavior
    // Equips each of the weapon arrays to our model
    private IEnumerator EquipAllWeapons()
    {
        // I chose to do a Coroutine to equip each weapon sequentially because of code that was used in 'GunController' that had equip delay
        yield return StartCoroutine(EquipWeaponArray(shotgunCannons, equippedShotguns, shotgunHardpoints));
        yield return StartCoroutine(EquipWeaponArray(sniperRifles, equippedSnipers, sniperHardpoints));
        yield return StartCoroutine(EquipWeaponArray(automaticRifles, equippedRifles, rifleHardpoints));
    }

    // Iterates through the weapon array to equip each weapon
    private IEnumerator EquipWeaponArray(GameObject[] weaponPrefabs, List<IWeapon> weaponList, Transform[] hardpoints)
    {
        // Loops through each weapon prefab in the provided array
        for (int index = 0; index < weaponPrefabs.Length; index++)
        {
            // Gets the IWeapon component from the current weapon prefab
            IWeapon weaponScript = weaponPrefabs[index].GetComponent<IWeapon>();

            // Checks if a valid IWeapon component was found
            if (weaponScript != null)
            {
                // Checks if the weapon is of type IGun, then initializes it to equip it
                if (weaponScript is IGun gun)
                {
                    // Initialize the gun's ammo with a random amount up to its capacity (it doesnt need to be random, I just found it easier to do it this way)
                    gun.InitializeAmmo(gun.GetMagAmmoCapacity(), Random.Range(0, gun.GetMagAmmoCapacity()));
                    // Wait for the specified equip time before proceeding
                    yield return new WaitForSeconds(gun.GetEquipTime());
                    gun.Equip();
                }
                // Adds the weapon script to the list of equipped weapons
                weaponList.Add(weaponScript);
            }
        }
    }
    #endregion

    // Handles the firing sequence and firing animation
    #region combat
    // Aims the weapon list at the players world position
    public void AimAtPlayer(Vector3 worldPosition)
    {
        AimWeaponListAt(equippedShotguns, worldPosition);
        AimWeaponListAt(equippedRifles, worldPosition);
        AimWeaponListAt(equippedSnipers, worldPosition);
    }

    // Aims the list of weapons at the world position of the player character
    private void AimWeaponListAt(List<IWeapon> weapons, Vector3 worldPosition)
    {
        foreach (var weapon in weapons)
        {
            weapon.AimAt(worldPosition);
        }
    }

    // this begins the firing routine of the equiped weapon prefabs based on the current phase as determined prior
    public void FireWeapons()
    {
        // Shotguns being the base weapon are always set to fire
        FireWeaponList(equippedShotguns);
        // The animation for the guns are called whenever they are fired
        animator.SetTrigger("Shoot Cannons");

        // Fires the equiped rifles during phase 2
        if (currentPhase >= 2)
        {
            FireWeaponList(equippedRifles);
        }

        // Fires the equiped sniper rifles in phase 3
        if (currentPhase == 3)
        {
            FireWeaponList(equippedSnipers);
        }
    }

    // The command to fire each weapon prefab
    private void FireWeaponList(List<IWeapon> weapons)
    {
        foreach (var weapon in weapons)
        {
            weapon.AttackStarted();
        }
    }
}
#endregion