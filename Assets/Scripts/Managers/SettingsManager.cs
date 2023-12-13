using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance { get; private set; }

    public float lookSensitivity;
    [SerializeField] Slider sensitivityBar;

    public float volume;
    [SerializeField] Slider volumeBar;



    private void Awake()
    {
        instance = this;
        volume = 100;
    }

    public void AdjustVolume()
    {
        volume = volumeBar.value;      
    }

    public void AdjustSensitivity()
    {
        lookSensitivity = sensitivityBar.value;
    }

}
