using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossManager : MonoBehaviour
{
    public static BossManager instance { get; private set; }

    public Image bossHP;

    void Start()
    {
        instance = this;
    }

    
}
