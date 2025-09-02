using UnityEngine;

[ExecuteAlways]
public class ScreenBoundsColliders2D : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float thickness = 1f;
    [SerializeField] float sidePadding = 0f;
    [SerializeField] float bottomPadding = 0f;
    [SerializeField] float topPadding = 0f;
    [SerializeField] bool includeCeiling = false;
    [SerializeField] string groundLayerName = "Ground";
    [SerializeField] bool collidersAsTriggers = false; // usually false for floor/walls

    BoxCollider2D floor, leftWall, rightWall, ceiling;

    void OnEnable()
    {
        if (!cam) cam = Camera.main;
        EnsureColliders();
        UpdateColliders();
    }

    void LateUpdate() => UpdateColliders();

    void EnsureColliders()
    {
        floor = GetOrMake("Floor");
        leftWall = GetOrMake("LeftWall");
        rightWall = GetOrMake("RightWall");
        ceiling = includeCeiling ? GetOrMake("Ceiling") : RemoveIfExists("Ceiling");

        int groundLayer = LayerMask.NameToLayer(groundLayerName);
        foreach (var c in new[] { floor, leftWall, rightWall, ceiling })
        {
            if (!c) continue;
            c.gameObject.layer = groundLayer >= 0 ? groundLayer : c.gameObject.layer;
            c.isTrigger = collidersAsTriggers;
        }
    }

    void UpdateColliders()
    {
        if (!cam || !floor || !leftWall || !rightWall) return;

        // World-space screen bounds
        float orthoSize = cam.orthographicSize;                 // half height
        float height = orthoSize * 2f;
        float width = height * cam.aspect;

        Vector3 camPos = cam.transform.position;
        float left = camPos.x - width / 2f;
        float right = camPos.x + width / 2f;
        float bottom = camPos.y - height / 2f;
        float top = camPos.y + height / 2f;

        // Floor
        SetBox(floor,
            center: new Vector2(camPos.x, bottom - thickness / 2f - bottomPadding),
            size:   new Vector2(width + 2f * thickness, thickness));

        // Left wall
        SetBox(leftWall,
            center: new Vector2(left - thickness / 2f - sidePadding, camPos.y),
            size:   new Vector2(thickness, height + 2f * thickness));

        // Right wall
        SetBox(rightWall,
            center: new Vector2(right + thickness / 2f + sidePadding, camPos.y),
            size:   new Vector2(thickness, height + 2f * thickness));

        // Ceiling (optional)
        if (includeCeiling && ceiling)
        {
            SetBox(ceiling,
                center: new Vector2(camPos.x, top + thickness / 2f + topPadding),
                size:   new Vector2(width + 2f * thickness, thickness));
        }
    }

    static void SetBox(BoxCollider2D box, Vector2 center, Vector2 size)
    {
        var t = box.transform;
        t.position = new Vector3(center.x, center.y, t.position.z);
        box.size = size;
        box.offset = Vector2.zero;
    }

    BoxCollider2D GetOrMake(string name)
    {
        var t = transform.Find(name);
        if (!t)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            t = go.transform;
        }
        var col = t.GetComponent<BoxCollider2D>();
        if (!col) col = t.gameObject.AddComponent<BoxCollider2D>();
        return col;
    }

    BoxCollider2D RemoveIfExists(string name)
    {
        var t = transform.Find(name);
        if (!t) return null;
        var col = t.GetComponent<BoxCollider2D>();
        if (col) DestroyImmediate(col.gameObject);
        return null;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        foreach (var c in new[] { floor, leftWall, rightWall, ceiling })
        {
            if (!c) continue;
            Gizmos.DrawCube(c.bounds.center, c.bounds.size);
        }
    }
#endif
}

