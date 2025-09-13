using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public sealed class PowerUpSpawner2D : MonoBehaviour
{
    [SerializeField] private GameObject[] powerUpPrefabs;   // ← multiple prefabs
    [SerializeField] private int spawnEveryPlatforms;
    [SerializeField] private float minLeadAbovePlayer = 2.5f; // world units above player
    private Transform player;
    private int nextSpawnFloor = 20;
    private bool spawnsJetpacks;

    // set  platform layers in the Inspector
    [SerializeField] private LayerMask platformLayers;

    // placement tuning
    [SerializeField] private float verticalOffset = 0.25f;
    [SerializeField] private float horizontalPadding = 0.3f;
    [SerializeField] private Vector2 overlapCheckSize = new Vector2(0.35f, 0.35f);
    [SerializeField] private int maxAttempts = 15;
    [SerializeField] private int maxConcurrent = 15;

    private readonly List<Collider2D> platforms = new();

    private void Awake()
    {
        // first target equals the cadence set in the Inspector
        nextSpawnFloor = spawnEveryPlatforms;
        Debug.Log($"[{name}] Awake → cadence={spawnEveryPlatforms}, next={nextSpawnFloor}");
    }

    private void Start()
    {
        CollectPlatforms();
        nextSpawnFloor = spawnEveryPlatforms;
        // Detect the pickup type this spawner manages (based on its prefabs)
        spawnsJetpacks = false;
        foreach (var p in powerUpPrefabs)
        {
            if (p != null && p.GetComponent<JetpackPickup2D>() != null)
            {
                spawnsJetpacks = true;
                break;
            }
        }
        if (spawnsJetpacks)
        {
            // ensure first jetpack target is floor 50
            nextSpawnFloor = 50;                     // or Mathf.Max(nextSpawnFloor, 50);
        }

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) player = playerGO.transform;
    }

    private void CollectPlatforms()
    {
        platforms.Clear();

#if UNITY_2023_1_OR_NEWER
        Collider2D[] all = UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
#else
		Collider2D[] all = UnityEngine.Object.FindObjectsOfType<Collider2D>();
#endif

        foreach (Collider2D c in all)
        {
            if (c.isTrigger) { continue; }
            if (((1 << c.gameObject.layer) & platformLayers) == 0) { continue; }
            platforms.Add(c);
        }
    }

    private bool IsJetpackSpawner()
    {
        // If any prefab in this spawner has a JetpackPickup2D, we treat this spawner as "jetpack"
        if (powerUpPrefabs == null) return false;
        for (int i = 0; i < powerUpPrefabs.Length; i++)
        {
            var go = powerUpPrefabs[i];
            if (!go) continue;
            if (go.GetComponent<JetpackPickup2D>() != null) return true;
        }
        return false;
    }

    private int GetAliveCountForThisSpawner()
    {
        bool isJetpack = IsJetpackSpawner();

#if UNITY_2023_1_OR_NEWER
        return isJetpack
            ? UnityEngine.Object.FindObjectsByType<JetpackPickup2D>(FindObjectsSortMode.None).Length
            : UnityEngine.Object.FindObjectsByType<PowerUpPickup2D>(FindObjectsSortMode.None).Length;
#else
	return isJetpack
		? UnityEngine.Object.FindObjectsOfType<JetpackPickup2D>().Length
		: UnityEngine.Object.FindObjectsOfType<PowerUpPickup2D>().Length;
#endif
    }

    private bool TrySpawn(out GameObject instance)
    {
        instance = null;

        // refresh in case platforms were added/removed at runtime
        CollectPlatforms();

        if (platforms.Count == 0 || powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            Debug.LogWarning("PowerUpSpawner2D: no platforms or prefabs assigned.");
            return false;
        }

        for (int i = 0; i < maxAttempts; i++)
        {
            Collider2D plat = platforms[Random.Range(0, platforms.Count)];
            Bounds b = plat.bounds;

            float left = b.min.x + horizontalPadding;
            float right = b.max.x - horizontalPadding;
            if (right <= left)
            {
                continue; // platform too narrow for padding
            }

            float x = Random.Range(left, right);
            float y = b.max.y + verticalOffset;
            Vector2 pos = new Vector2(x, y);

            // avoid overlapping platforms at the spawn point
            if (Physics2D.OverlapBox(pos, overlapCheckSize, 0f, platformLayers) != null)
            {
                continue;
            }

            GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
            instance = Instantiate(prefab, pos, Quaternion.identity);
            Debug.Log($"[Spawner] Spawned at {pos} using {prefab.name}");
            return true;
        }

        return false;
    }

    public void MaybePreplaceForFloor(int floorIndex, Collider2D platform)
    {
        Debug.Log($"[{name}] Preplace check f={floorIndex}, next={nextSpawnFloor}, every={spawnEveryPlatforms}");
        // only at 20, 40, 60, ... (or whatever you set)
        if (floorIndex < nextSpawnFloor) return;


        int alive = GetAliveCountForThisSpawner();
        // -1 = unlimited
        if (maxConcurrent >= 0 && alive >= maxConcurrent) return;

        if (platform == null) return;

        // choose a point above this platform
        Bounds b = platform.bounds;
        float left = b.min.x + horizontalPadding;
        float right = b.max.x - horizontalPadding;
        if (right <= left) return;  // platform too narrow for our padding

        float y = b.max.y + verticalOffset;
        if (player != null)
        {
            // ensure the pickup is not “too late” – keep it ahead of the player
            y = Mathf.Max(y, player.position.y + minLeadAbovePlayer);
        }
        Vector2 pos = new Vector2(Random.Range(left, right), y);

        // avoid overlaps with platform or other power-ups
        if (Physics2D.OverlapBox(pos, overlapCheckSize, 0f, platformLayers) != null) return;
        if (Physics2D.OverlapBox(pos, overlapCheckSize, 0f, LayerMask.GetMask("PowerUp")) != null) return;

        // pick a prefab and spawn
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        GameObject prefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];
        Instantiate(prefab, pos, Quaternion.identity);

        // schedule the next milestone (e.g., 20 → 40 → 60 ...)
        nextSpawnFloor += spawnEveryPlatforms;
    }

    public void NotifyReachedFloor(int floorIndex)
    {
        Debug.Log($"[{name}] NotifyReachedFloor f={floorIndex}, next={nextSpawnFloor}, every={spawnEveryPlatforms}");
        if (floorIndex < nextSpawnFloor) return;

        int alive = GetAliveCountForThisSpawner();
        // -1 = unlimited
        if (maxConcurrent >= 0 && alive >= maxConcurrent) return;

        if (TrySpawn(out _))
        {
            nextSpawnFloor += spawnEveryPlatforms;
        }
    }


}
