// using UnityEngine;

// public class CameraFollow : MonoBehaviour
// {
//     [SerializeField] private GameObject targetObj;
//     [SerializeField] private float startThreshold = 5f;
//     [SerializeField] private float catchUpThreshold = 1f;
//     [SerializeField] private float followSpeed = 2f;

//     void LateUpdate()
//     {
//         Transform target = targetObj.transform;
        
//         // Transform target = targetObj.transform;
//         if (!target) return;

//         float distance = target.position.y - transform.position.y;

//         if (target.position.y < startThreshold) return;

//         // Decide desired position
//         Vector3 desiredPos = (distance > catchUpThreshold)
//             ? new Vector3(0f, target.position.y, transform.position.z) // catch up if player is much above
//             : new Vector3(0f, transform.position.y + followSpeed, transform.position.z); // steady pace

//         float t = (distance > catchUpThreshold) ? distance * Time.deltaTime : Time.deltaTime;
//         transform.position = Vector3.Lerp(transform.position, desiredPos, t);
//     }
// }
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private GameObject targetObj;

    [Header("Thresholds")]
    [SerializeField] private float startThreshold = 5f;
    [SerializeField] private float catchUpThreshold = 1f;

    [Header("Camera Speed Settings")]
    [SerializeField] private float baseSpeed = 2f;          // starting speed
    [SerializeField] private float speedStep = 0.5f;        // how much to increase every interval
    [SerializeField] private float incrementInterval = 30f; // seconds per increment
    [SerializeField] private float maxSpeed = 10f;          // cap

    private float currentSpeed;
    private float timer;

    void Start()
    {
        currentSpeed = baseSpeed;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= incrementInterval)
        {
            currentSpeed = Mathf.Min(currentSpeed + speedStep, maxSpeed);
            timer = 0f;
            Debug.Log($"Camera speed increased: {currentSpeed}");
            AudioManager.Instance?.PlayCameraSpeedUp();
        }
    }

    void LateUpdate()
    {
        if (!targetObj) return;
        Transform target = targetObj.transform;

        float distance = target.position.y - transform.position.y;

        if (target.position.y < startThreshold) return;

        Vector3 desiredPos = (distance > catchUpThreshold)
            ? new Vector3(0f, target.position.y, transform.position.z)
            : new Vector3(0f, transform.position.y + currentSpeed * Time.deltaTime, transform.position.z);

        float t = (distance > catchUpThreshold)
            ? distance * Time.deltaTime
            : 1f;

        transform.position = Vector3.Lerp(transform.position, desiredPos, t);
    }
}
