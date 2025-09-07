using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class FollowFloor2D : MonoBehaviour
{
    public Camera cam;                 // leave empty to auto-grab Main Camera
    public Transform followX;          // usually the player; if null, uses camera.x
    public float thickness = 0.5f;     // collider height
    public float extraWidth = 4f;      // extend beyond screen edges
    public float yPadding = 0.02f;     // slight offset so it doesn’t clip sprites

    Rigidbody2D rb;
    BoxCollider2D col;
    Vector2 desiredPos;                // computed in LateUpdate, applied in FixedUpdate

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        col = GetComponent<BoxCollider2D>();
        col.isTrigger = false;

        if (!cam) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (!cam) return;

        // Compute screen width in world units (orthographic)
        float halfWidth = cam.orthographicSize * cam.aspect;

        // Resize collider to fill width + padding
        col.size = new Vector2(halfWidth * 2f + extraWidth, thickness);

        // Bottom of the camera in world Y
        float bottomY = cam.transform.position.y - cam.orthographicSize + (thickness * 0.5f) - yPadding;

        // Follow player horizontally (if assigned), else the camera
        float x = followX ? followX.position.x : cam.transform.position.x;

        desiredPos = new Vector2(x, bottomY);
    }

    void FixedUpdate()
    {
        rb.MovePosition(desiredPos);
    }

    // Optional debug to confirm contact:
    void OnCollisionStay2D(Collision2D c)
    {
        // Debug.Log("Floor contact with: " + c.collider.name);
    }
}

