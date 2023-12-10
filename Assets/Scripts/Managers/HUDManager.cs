using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions.Must;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance { get; private set; }

    [Header("-----Ammo-----")]
    [SerializeField] TMP_Text magAmmoText;
    [SerializeField] TMP_Text reserveAmmoText;
    public GameObject ammoCount;

    [Header("-----Enemies-----")]
    public TMP_Text enemyCountText;
    public GameObject enemiesBackground;
    int enemiesRemaining;

    [Header("-----Health-----")]
    public Image playerHPBar;
    public GameObject playerHPBarFrame;

    [Header("-----Minimap-----")]
    public GameObject minimap;

    [Header("-----ScreenStatus-----")]
    public GameObject statusObject;
    [SerializeField] Image statusScreen;
    [SerializeField] CanvasGroup statusScreenAlpha;
    public GameObject hitMarker;

    [Header("-----Reticle-----")]
    public ReticleController reticleController;

    [Header("-----General-----")]
    public GameManager.GameStates currState;
    
   
    void Awake()
    {
        instance = this;
    }

    #region "Show" Functions
    public void ShowHUD()
    {
        playerHPBarFrame.SetActive(true);
        ammoCount.SetActive(true);
        enemiesBackground.SetActive(true);
        reticleController.gameObject.SetActive(true);
        statusObject.SetActive(true);
        minimap.SetActive(true);
    }

    public void ShowReticle()
    {
        reticleController.gameObject.SetActive(true);
    }

    public void ShowAmmoCount()
    {
        ammoCount.SetActive(true);
    }

    public void ShowHitMarker()
    {
        hitMarker.SetActive(true);
    }

    #endregion

    #region "Hide" Functions
    public void HideHUD()
    {
        playerHPBarFrame.SetActive(false);
        ammoCount.SetActive(false);
        enemiesBackground.SetActive(false);
        reticleController.gameObject.SetActive(false);
        statusObject.SetActive(false);
        minimap.SetActive(false);
    }

    void HideAmmoCount()
    {
        ammoCount.SetActive(false);
    }

    #endregion

    #region Update Functions
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
            StartCoroutine(UIManager.instance.YouWin());
        }
    }

    #endregion

    #region Status Functions
    public IEnumerator FlashColor(float targetAlpha, float totalTime, Color color)
    {
        statusScreen.color = color;
        statusScreenAlpha.alpha = 1;
        float currTime = 0f;
        float startAlpha = statusScreenAlpha.alpha;

        while (currTime < totalTime)
        {
            statusScreenAlpha.alpha = Mathf.Lerp(startAlpha, targetAlpha, currTime / totalTime);
            currTime += Time.deltaTime;
            yield return null;
        }
    }

    /* -----Function for showing hitmarkers-----
    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            GameObject marker = Instantiate(HUDManager.instance.hitMarker, HUDManager.instance.hitMarker.transform);
            Destroy(marker, 0.2f);
        }
    }
    */

    #endregion
}
