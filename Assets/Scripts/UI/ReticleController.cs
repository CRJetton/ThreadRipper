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

    void LateUpdate()
    {
        CompressReticle();
    }

    void CompressReticle()
    {
        if(top.transform.position.y > topStartPos.y)
        {
            top.transform.position = new Vector3(top.transform.position.x, top.transform.position.y - resetSpeed, top.transform.position.z);
        }

        if(bottom.transform.position.y < bottomStartPos.y)
        {
            bottom.transform.position = new Vector3(bottom.transform.position.x, bottom.transform.position.y - resetSpeed, bottom.transform.position.z);
        }

        if(left.transform.position.x < leftStartPos.x)
        {
            left.transform.position = new Vector3(left.transform.position.x - resetSpeed, left.transform.position.y, left.transform.position.z);
        }

        if(right.transform.position.x > rightStartPos.x)
        {
            right.transform.position = new Vector3(right.transform.position.x - resetSpeed, right.transform.position.y, right.transform.position.z);
        }
    }

    public void ExpandReticle(float amount)
    {
        top.transform.position = new Vector3(top.transform.position.x, top.transform.position.y + amount, top.transform.position.z);
        bottom.transform.position = new Vector3(bottom.transform.position.x, bottom.transform.position.y - amount, bottom.transform.position.z);
        left.transform.position = new Vector3(left.transform.position.x - amount, left.transform.position.y, left.transform.position.z);
        right.transform.position = new Vector3(right.transform.position.x + amount, right.transform.position.y, right.transform.position.z);
    }

    public void SetSpeed(float speed)
    {
        resetSpeed = speed;
    }
}
