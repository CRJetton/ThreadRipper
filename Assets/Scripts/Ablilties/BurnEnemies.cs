using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnEnemies : MonoBehaviour
{
    public StatusEffects BurnEffectPrefab;

    public void StartBurn(GameObject enemy)
    {
        //apply burn effect
        ApplyBurnStatus(enemy);
    }

    public void ApplyBurnStatus(GameObject enemy)
    {
        //access the enemy's StatusEffectController component
        StatusEffectController statusController = enemy.GetComponent<StatusEffectController>();
        if (statusController != null)
        {
            //create instance of burn effect
            StatusEffects burnEffect = Instantiate(BurnEffectPrefab);

            //apply the burn effect
            statusController.ApplyStatusEffect(burnEffect);
        }
    }

}
