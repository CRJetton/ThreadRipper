using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupMenu : MonoBehaviour
{
    
   
    void Update()
    {
        UIManager.instance.playerDirection = GameManager.instance.playerBodyPositions.playerHead.position - UIManager.instance.popupMenu.transform.position;
        UIManager.instance.PopupFacePlayer();     
    }
}
