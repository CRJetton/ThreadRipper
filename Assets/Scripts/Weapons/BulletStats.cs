using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BulletStats : ScriptableObject
{
    public string noHitTag;

    public float damage;
    public float speed;
    public float destroyTime;
}
