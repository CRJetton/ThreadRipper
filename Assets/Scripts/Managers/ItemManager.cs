using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public enum ItemType
    {
        pistol,
        rifle,
        shotgun,
        smg,
        sniper,

        smallHealth = 20,
        medHealth,
        largeHealth
    }

    Dictionary<ItemType, string> itemNames = new Dictionary<ItemType, string>()
    {
        { ItemType.pistol, "Pistol" },
        { ItemType.rifle, "Rifle" },
        { ItemType.shotgun, "Shotgun" },
        { ItemType.smg, "SMG" },
        { ItemType.sniper, "Sniper"},

        { ItemType.smallHealth, "Health - S"},
        { ItemType.medHealth, "Health - M" },
        { ItemType.largeHealth, "Health - L" }
    };

    public static ItemManager instance { get; private set; }

    [SerializeField] float lookDistance;

    [SerializeField] Image itemPopup;
    [SerializeField] Text itemName;

    LayerMask itemPickupMask;

    void Awake()
    {
        instance = this;
        itemPickupMask = LayerMask.GetMask("Pickup");
    }
    
    void PopupItemMenu()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, lookDistance, itemPickupMask))
        {
            itemPopup.gameObject.SetActive(true);
        }
        else
        {
            itemPopup.gameObject.SetActive(false);
        }
    }
    
   
}
