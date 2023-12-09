using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions.Must;

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

    public GameObject statusObject;
    [SerializeField] Image statusScreen;
    [SerializeField] CanvasGroup statusScreenAlpha;
    
 
    public ReticleController reticleController;

    public GameManager.GameStates currState;
    int enemiesRemaining;

    void Awake()
    {
        instance = this;

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
            StartCoroutine(UIManager.instance.YouWin());
        }

    }

    public IEnumerator FlashColor(float targetAlpha, float totalTime, Color color)
    {
        statusScreen.color = color;
        statusScreenAlpha.alpha = 1;
        float currTime = 0f;
        float startAlpha = statusScreenAlpha.alpha;

        while(currTime < totalTime)
        {
            statusScreenAlpha.alpha = Mathf.Lerp(startAlpha, targetAlpha, currTime / totalTime);
            currTime += Time.deltaTime;
            yield return null;
        }
    }
   
}
