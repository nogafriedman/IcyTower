using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject targetObj;
    [SerializeField] private float startThreshold = 5f;
    [SerializeField] private float catchUpThreshold = 1f;
    [SerializeField] private float followSpeed = 2f;

    void LateUpdate()
    {
        Transform target = targetObj.transform;
        if (!target) return;

        float distance = target.position.y - transform.position.y;

        if (target.position.y < startThreshold) return;

        // Decide desired position
        Vector3 desiredPos = (distance > catchUpThreshold)
            ? new Vector3(0f, target.position.y, transform.position.z) // catch up if player is much above
            : new Vector3(0f, transform.position.y + followSpeed, transform.position.z); // steady pace

        float t = (distance > catchUpThreshold) ? distance * Time.deltaTime : Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, desiredPos, t);
    }
}

