using System.Collections.Generic;
using UnityEngine;

public class ProgressGenerator : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform target;

    [Header("Side Walls")]
    [SerializeField] private GameObject wallSegmentPrefab;
    [SerializeField] private float wallSegmentHeight = 11.5f;
    [SerializeField] private float wallSpawnAhead = 10f;
    [SerializeField] private int prewarmWallSegments = 6;

    [Header("Platforms")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private float platformSpacing = 2f;
    [SerializeField] private float platformSpawnAhead = 10f;
    [SerializeField] private int prewarmPlatforms = 10;
    [SerializeField] private float platformXMin = -5f;
    [SerializeField] private float platformXMax = 5f;

    // runtime state
    private readonly List<GameObject> _wallPool = new List<GameObject>();
    private readonly List<GameObject> _platformPool = new List<GameObject>();
    private float _nextWallY;
    private float _nextPlatformY;

    private void Awake()
    {
        SeedWalls();
        SeedPlatforms();
    }

    private void Update()
    {
        // When the highest planned Y is close to the player, recycle/spawn the next piece
        if (_nextWallY - target.position.y < wallSpawnAhead)
            RecycleWall();

        if (_nextPlatformY - target.position.y < platformSpawnAhead)
            RecyclePlatform();
    }

    // ---- Initialization ----

    private void SeedWalls()
    {
        _wallPool.Capacity = Mathf.Max(_wallPool.Capacity, prewarmWallSegments);

        for (int i = 0; i < prewarmWallSegments; i++)
        {
            var pos = new Vector3(0f, _nextWallY, 0f);
            var segment = Instantiate(wallSegmentPrefab, pos, Quaternion.identity, transform);
            _wallPool.Add(segment);
            _nextWallY += wallSegmentHeight;
        }
    }

    private void SeedPlatforms()
    {
        _platformPool.Capacity = Mathf.Max(_platformPool.Capacity, prewarmPlatforms);

        for (int i = 0; i < prewarmPlatforms; i++)
        {
            var x = Random.Range(platformXMin, platformXMax);
            var pos = new Vector3(x, _nextPlatformY, 0f);
            var p = Instantiate(platformPrefab, pos, Quaternion.identity, transform);
            _platformPool.Add(p);
            _nextPlatformY += platformSpacing;
        }
    }

    // ---- Recycling / Spawning ----

    private void RecycleWall()
    {
        // Move the oldest to the newest Y and push it to the back of the ring
        var segment = _wallPool[0];
        _wallPool.RemoveAt(0);

        segment.transform.position = new Vector3(0f, _nextWallY, 0f);
        _wallPool.Add(segment);

        _nextWallY += wallSegmentHeight;
    }

    private void RecyclePlatform()
    {
        var platform = _platformPool[0];
        _platformPool.RemoveAt(0);

        var x = Random.Range(platformXMin, platformXMax);
        platform.transform.position = new Vector3(x, _nextPlatformY, 0f);
        _platformPool.Add(platform);

        _nextPlatformY += platformSpacing;
    }
}
