using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour, IDamageable
{
    [SerializeField] float health;
    [SerializeField] Color hitColor;
    [SerializeField] Renderer model;

    Color defaultColor;


    void Start()
    {
        defaultColor = model.material.color;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
        }

        StartCoroutine(FlashOnHit());
    }

    IEnumerator FlashOnHit()
    { 
        model.material.color = hitColor;

        yield return new WaitForSeconds(0.05f);

        model.material.color = defaultColor;
    }
}
