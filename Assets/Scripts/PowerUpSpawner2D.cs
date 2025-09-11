using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PowerUpSpawner2D : MonoBehaviour
{
    [SerializeField] private GameObject[] powerUpPrefabs;   // ← multiple prefabs
    [SerializeField] private float spawnIntervalSeconds = 12f;

    // set  platform layers in the Inspector
    [SerializeField] private LayerMask platformLayers;

    // placement tuning
    [SerializeField] private float verticalOffset = 0.25f;
    [SerializeField] private float horizontalPadding = 0.3f;
    [SerializeField] private Vector2 overlapCheckSize = new Vector2(0.35f, 0.35f);
    [SerializeField] private int maxAttempts = 15;
    [SerializeField] private int maxConcurrent = 1;

    private readonly List<Collider2D> platforms = new();
    private int activeCount;

    private void Start()
    {
        CollectPlatforms();
        StartCoroutine(SpawnLoop());
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

    private IEnumerator SpawnLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(spawnIntervalSeconds);

        while (true)
        {
            if (activeCount < maxConcurrent && TrySpawn(out GameObject obj))
            {
                activeCount++;
                OnDestroyNotify watcher = obj.AddComponent<OnDestroyNotify>();
                watcher.OnDestroyed += () => activeCount--;
            }
            yield return wait;
        }
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
            return true;
        }

        return false;
    }


    private sealed class OnDestroyNotify : MonoBehaviour
    {
        public System.Action OnDestroyed;
        private void OnDestroy() => OnDestroyed?.Invoke();
    }
}
