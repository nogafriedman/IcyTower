using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject target;

    private void LateUpdate()
    {
        if(target.transform.position.y > transform.position.y)
        {
            Vector3 newPosition = new Vector3(transform.position.x, target.transform.position.y, transform.position.z);
            transform.position = newPosition;
        }
    }
   
}
