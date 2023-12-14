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
    [SerializeField] AudioSource levelMusic;



    private void Awake()
    {
        instance = this;
        
    }

    public void AdjustVolume()
    {
        volume = volumeBar.value;
        levelMusic.volume = volume;
        
    }

    public void AdjustSensitivity()
    {
        lookSensitivity = sensitivityBar.value;
    }

}
