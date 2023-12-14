using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossManager : MonoBehaviour
{
    public static BossManager instance { get; private set; }

    public Image bossHP;
    public GameObject bossHealth;
   

    void Start()
    {
        instance = this;
    }

    public void ShowBossHealth()
    {
        bossHealth.SetActive(true);
    }
}
