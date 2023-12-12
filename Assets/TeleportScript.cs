using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    [SerializeField] float teleSpeed;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //makes the teleport pos move
        transform.position += transform.forward * teleSpeed * Time.deltaTime;
    }
}
