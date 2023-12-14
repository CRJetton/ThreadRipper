using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager instance { get; private set; }
    [SerializeField] private GameObject[] environmentGameObjects;
    [SerializeField] private List<IEnvironment> environmentInterfaces = new List<IEnvironment>();

    void Awake()
    {
        instance = this;

        environmentGameObjects = GameObject.FindGameObjectsWithTag("IEnvironment");
        
        if (environmentGameObjects.Length > 0 )
        {
            for (int i = 0; i < environmentGameObjects.Length; ++i)
            {
                if (environmentGameObjects[i].GetComponent<IEnvironment>() != null)
                {
                    environmentInterfaces.Add(environmentGameObjects[i].GetComponent<IEnvironment>());
                }
            }
        }
    }

    public List<IEnvironment> GetEnvironmentInterfaces()
    {
        return environmentInterfaces;
    }
}
