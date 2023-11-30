using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance { get; private set; }
    [SerializeField] TMP_Text magAmmoText;
    [SerializeField] TMP_Text reserveAmmoText;
    public TMP_Text enemyCountText;
    public Image playerHPBar;

    int enemiesRemaining;

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

    public void UpdateProgress(int amount)
    {
        enemiesRemaining += amount;
        enemyCountText.text = enemiesRemaining.ToString("0");

        if (enemiesRemaining <= 0)
        {
            UIManager.instance.StatePaused();
            UIManager.instance.menuActive = UIManager.instance.menuWin;
            UIManager.instance.menuActive.SetActive(true);
        }

    }
}
