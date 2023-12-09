using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance { get; private set; }

    [SerializeField] float lookDistance;

    [SerializeField] Image itemPopup;

    void Awake()
    {
        instance = this;
    }

    
    
    /*bool LookingAtItem()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, lookDistance,))
        {
            itemPopup.gameObject.SetActive(true);
        }
    }
    */
   
}
