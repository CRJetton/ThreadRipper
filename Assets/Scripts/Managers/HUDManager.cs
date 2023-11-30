using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;

    public TMP_Text enemyCountText;
    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        
    }
}
