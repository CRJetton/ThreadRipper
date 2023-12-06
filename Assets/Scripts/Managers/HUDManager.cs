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
    public GameObject playerHPBarFrame;
    public GameObject ammoCount;
    public GameObject enemiesBackground;
    public GameObject minimap;

    public GameObject damageScreen;
    [SerializeField][Range(0f, 1.0f)] float damageAlpha;


    public ReticleController reticleController;

    Color damageColor;

    int enemiesRemaining;

    void Awake()
    {
        instance = this;
        damageColor = damageScreen.GetComponent<Image>().color;

    }

    void Update()
    {
        if (damageColor.a > 0)
        {
            damageColor.a -= 0.01f;
            damageScreen.GetComponent<Image>().color = damageColor;
        }

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

    public void FlashDamage()
    {
        damageColor.a = damageAlpha;

        damageScreen.GetComponent<Image>().color = damageColor;
    }

}
