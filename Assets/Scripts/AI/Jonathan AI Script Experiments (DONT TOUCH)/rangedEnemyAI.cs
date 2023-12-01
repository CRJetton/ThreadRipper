using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyCombat))]
public class rangedEnemyAI : AbstractAI
{

    [SerializeField] EnemyCombat enemyCombat;

    private void Awake()
    {
        enemyCombat = GetComponent<EnemyCombat>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }    

    public override void Combat()
    {

    }

}
