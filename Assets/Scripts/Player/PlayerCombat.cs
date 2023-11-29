using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] float maxAimRange;
    [SerializeField] float minAimRange;

    [SerializeField] Transform weaponContainer;
    [SerializeField] GameObject startingWeaponPrefab;
    private IWeapon weaponCurrent;


    private void Start()
    {
        Instantiate(startingWeaponPrefab, weaponContainer);

        weaponCurrent = weaponContainer.GetComponentInChildren<IWeapon>();
    }


    private void LateUpdate()
    {
        AimControl();
        AttackControl();
    }


    void AimControl()
    {
        if (weaponCurrent != null)
        {
            RaycastHit hit;
            Ray centerScreen = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            Vector3 centerPos;

            if (Physics.Raycast(centerScreen, out hit, maxAimRange) && hit.distance > minAimRange)
            {
                Debug.DrawLine(Camera.main.transform.position, hit.point, Color.blue);
                centerPos = hit.point;
            }
            else
            {
                centerPos = Camera.main.transform.position + Camera.main.transform.forward * maxAimRange;
            }

            weaponCurrent.AimAt(centerPos);
        }
    }

    void AttackControl()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (weaponCurrent != null)
            {
                weaponCurrent.StartAttack();
            }
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            if (weaponCurrent != null)
            {
                weaponCurrent.StopAttack();
            }
        }
    }
}
