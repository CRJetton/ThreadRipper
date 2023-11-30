using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance { get; private set; }
    [SerializeField] TMP_Text magAmmoText;
    [SerializeField] TMP_Text reserveAmmoText;

    public TMP_Text enemyCountText;
    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        
    }

    public void UpdateAmmoCount(int magAmmo, int reserveAmmo)
    {
        magAmmoText.text = magAmmo.ToString();
        reserveAmmoText.text = reserveAmmo.ToString();
    }
}
