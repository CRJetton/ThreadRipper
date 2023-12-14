using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExitLight: MonoBehaviour, IEnvironment
{
    [SerializeField] private Renderer exitLight;
    [SerializeField] private Material unlockedMaterial;

    public void OnGameGoalComplete()
    {
        exitLight.material = unlockedMaterial;
    }
}
