using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    //List Of Active Effects
private List<StatusEffects> activeEffect = new List<StatusEffects>();

    //Code To Apply Effects
    public void ApplyStatusEffect(StatusEffects effect)
    {
        //Adds The Effect To The Enemy
        activeEffect.Add(effect);
        //Starts The Duration Timer
        StartCoroutine(Timer(effect));
    }

    //Timer For The Effects
    IEnumerator Timer(StatusEffects effect)
    {
        //Waits For The Duration To Elapse
        yield return new WaitForSeconds(effect.duration);
        //Removes The Effect
        RemoveStatusEffect(effect);
    }

    //Code To Remove Effects
    public void RemoveStatusEffect(StatusEffects effect)
    {
        //removes The Effect From The Enemy
        activeEffect.Remove(effect);
    }
}
