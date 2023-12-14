using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffects")]
public class StatusEffects : ScriptableObject
{
    public string effectName;
    [SerializeField] public float duration;
    [SerializeField] public float damageOverTime;
}
