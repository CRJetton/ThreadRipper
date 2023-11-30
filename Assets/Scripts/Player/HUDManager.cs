using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance { get; private set; }

    [SerializeField] TMP_Text magAmmoText;
    [SerializeField] TMP_Text reserveAmmoText;


    private void Start()
    {
        instance = this;
    }


    public void UpdateAmmoCount(int magAmmo, int reserveAmmo)
    {
        magAmmoText.text = magAmmo.ToString();
        reserveAmmoText.text = reserveAmmo.ToString();
    }
}
