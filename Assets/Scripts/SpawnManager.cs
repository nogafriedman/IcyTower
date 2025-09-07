using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Walls")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float wallHeight = 10f;
    [SerializeField] private int initialWallCount = 4;
    [SerializeField] private float wallSpawnAhead = 10f;

    [Header("Platforms")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private int initialPlatformCount = 10;
    [SerializeField] private float platformSpawnAhead = 10f;
    [SerializeField] private float platformSpacing = 2f;
    [SerializeField] private float platformXMin = -5f;
    [SerializeField] private float platformXMax = 5f;

    private int nextFloorIndex = 1;

    private readonly List<GameObject> leftWallPool = new List<GameObject>();
    private readonly List<GameObject> rightWallPool = new List<GameObject>();
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

        while (nextWallY - player.position.y < wallSpawnAhead)
            RespawnWall();

        while (nextPlatformY - player.position.y < platformSpawnAhead)
            RespawnPlatform();
    }

    private void InitWalls()
    {
        float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;

        for (int i = 0; i < initialWallCount; i++)
        {
            var left = Instantiate(wallPrefab, new Vector3(-screenHalfWidth, nextWallY, 0f), Quaternion.identity, transform);
            leftWallPool.Add(left);

            var right = Instantiate(wallPrefab, new Vector3(+screenHalfWidth, nextWallY, 0f), Quaternion.identity, transform);
            rightWallPool.Add(right);

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
        float screenHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;

        var left = leftWallPool[0];
        leftWallPool.RemoveAt(0);
        left.transform.position = new Vector3(-screenHalfWidth, nextWallY, 0f);
        leftWallPool.Add(left);

        var right = rightWallPool[0];
        rightWallPool.RemoveAt(0);
        right.transform.position = new Vector3(+screenHalfWidth, nextWallY, 0f);
        rightWallPool.Add(right);

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
