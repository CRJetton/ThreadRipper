using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleController : MonoBehaviour
{
    [SerializeField] RawImage top;
    [SerializeField] RawImage bottom;
    [SerializeField] RawImage left;
    [SerializeField] RawImage right;

    [SerializeField] float expandAmountFactor;

    Vector3 topStartPos;
    Vector3 bottomStartPos;
    Vector3 leftStartPos;
    Vector3 rightStartPos;

    float resetSpeed;

    void Start()
    {
        topStartPos = top.transform.position;
        bottomStartPos = bottom.transform.position;
        leftStartPos = left.transform.position;
        rightStartPos = right.transform.position;
        
    }

    public void ExpandReticle(float amount)
    {
        amount *= expandAmountFactor;

        top.transform.position = new Vector3(topStartPos.x, topStartPos.y + amount, topStartPos.z);
        bottom.transform.position = new Vector3(bottomStartPos.x, bottomStartPos.y - amount, bottomStartPos.z);
        left.transform.position = new Vector3(leftStartPos.x - amount, leftStartPos.y, leftStartPos.z);
        right.transform.position = new Vector3(rightStartPos.x + amount, rightStartPos.y, rightStartPos.z);
    }

    public void SetSpeed(float speed)
    {
        resetSpeed = speed;
    }
}
