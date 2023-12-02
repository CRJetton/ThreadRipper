using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance { get; private set; }

    public float lookSensitivity;


    private void Awake()
    {
        instance = this;
    }
}
