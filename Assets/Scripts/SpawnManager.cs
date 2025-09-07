using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Walls")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float wallHeight = 100f;
    [SerializeField] private int initialWallCount = 10;
    [SerializeField] private float wallSpawnAhead = 10f;

    [Header("Platforms")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private int initialPlatformCount = 10;
    [SerializeField] private float platformSpawnAhead = 10f;
    [SerializeField] private float platformSpacing = 2f;
    [SerializeField] private float platformXMin = -5f;
    [SerializeField] private float platformXMax = 5f;

    private int nextFloorIndex = 1;

    private readonly List<GameObject> wallPool = new List<GameObject>();
    private readonly List<GameObject> platformPool = new List<GameObject>();
    private float nextWallY;
    private float nextPlatformY;

    private void Awake()
    {
        InitWalls();
        InitPlatforms();
    }

    private void Update()
    {
        if (nextWallY - player.position.y < wallSpawnAhead)
            RespawnWall();

        if (nextPlatformY - player.position.y < platformSpawnAhead)
            RespawnPlatform();
    }

    private void InitWalls()
    {
        wallPool.Capacity = Mathf.Max(wallPool.Capacity, initialWallCount);

        for (int i = 0; i < initialWallCount; i++)
        {
            var pos = new Vector3(0f, nextWallY, 0f);
            var segment = Instantiate(wallPrefab, pos, Quaternion.identity, transform);
            wallPool.Add(segment);
            nextWallY += wallHeight;
        }
    }

    private void InitPlatforms()
    {
        for (int i = 0; i < initialPlatformCount; i++)
        {
            var x = Random.Range(platformXMin, platformXMax);
            var platform = Instantiate(platformPrefab, new Vector3(x, nextPlatformY, 0f), Quaternion.identity, transform);

            var Indextag = platform.GetComponent<PlatformIndex>() ?? platform.AddComponent<PlatformIndex>();
            Indextag.floorIndex = nextFloorIndex++;

            platformPool.Add(platform);
            nextPlatformY += platformSpacing;
        }
    }

    private void RespawnWall()
    {
        var wall = wallPool[0];
        wallPool.RemoveAt(0);

        wall.transform.position = new Vector3(0f, nextWallY, 0f);
        wallPool.Add(wall);

        nextWallY += wallHeight;
    }

    private void RespawnPlatform()
    {
        var platform = platformPool[0];
        platformPool.RemoveAt(0);

        var x = Random.Range(platformXMin, platformXMax);
        platform.transform.position = new Vector3(x, nextPlatformY, 0f);
        platformPool.Add(platform);

        var indexTag = platform.GetComponent<PlatformIndex>() ?? platform.AddComponent<PlatformIndex>();
        indexTag.floorIndex = nextFloorIndex++;

        nextPlatformY += platformSpacing;
    }
}
