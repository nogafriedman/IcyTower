using UnityEngine;

[ExecuteAlways]
public class ScreenBoundsFloorCeiling2D : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float thickness = 1f;
    [SerializeField] float bottomPadding = 0f;
    [SerializeField] float topPadding = 0f;
    [SerializeField] bool includeCeiling = false;

    [Header("Layers & Options")]
    [SerializeField] string floorLayerName = "Ground";
    [SerializeField] string ceilingLayerName = "Ground";
    [SerializeField] bool collidersAsTriggers = false;

    BoxCollider2D floor, ceiling;

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
        ceiling = includeCeiling ? GetOrMake("Ceiling") : RemoveIfExists("Ceiling");

        // Force layers
        int floorLayer = LayerMask.NameToLayer(floorLayerName);
        int ceilLayer  = LayerMask.NameToLayer(ceilingLayerName);

        if (floor && floorLayer >= 0)   floor.gameObject.layer = floorLayer;
        if (ceiling && ceilLayer >= 0)  ceiling.gameObject.layer = ceilLayer;

        if (floor)   floor.isTrigger = collidersAsTriggers;
        if (ceiling) ceiling.isTrigger = collidersAsTriggers;
    }

    void UpdateColliders()
    {
        if (!cam || !floor) return;

        float orthoSize = cam.orthographicSize; // half height
        float height = orthoSize * 2f;
        float width = height * cam.aspect;
        Vector3 camPos = cam.transform.position;

        float bottom = camPos.y - height / 2f;
        float top    = camPos.y + height / 2f;

        // Floor
        SetBox(floor,
            center: new Vector2(camPos.x, bottom - thickness / 2f - bottomPadding),
            size:   new Vector2(width + 2f * thickness, thickness));

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
}

