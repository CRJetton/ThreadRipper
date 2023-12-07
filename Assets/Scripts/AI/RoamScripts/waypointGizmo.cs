using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waypointGizmo : MonoBehaviour
{
    [Range(1f, 3f)][SerializeField] int waypointRad;


    private void OnDrawGizmos()
    {
        foreach (Transform t in transform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(t.position, waypointRad);
        }

        for (int i = 0; i < transform.childCount; i++) 
        {
            Transform t = transform.GetChild(i);
            if (i == transform.childCount - 1)
            {
                Debug.DrawLine(t.position, transform.GetChild(0).position, Color.red);
            }
            else
                Debug.DrawLine(t.position, transform.GetChild(i+1).position, Color.red);
        }
    }
}
