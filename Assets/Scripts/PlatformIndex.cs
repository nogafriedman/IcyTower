using UnityEngine;

public class PlatformIndex : MonoBehaviour
{
    public int floorIndex = 0;
    private void Start()
	{
		PowerUpSpawner2D spawner = FindFirstObjectByType<PowerUpSpawner2D>();
		if (spawner == null)
		{
			return;
		}

		Collider2D col = GetComponent<Collider2D>();
		if (col == null)
		{
			return;
		}

		// spawner enforces 20/40/60… internally via nextSpawnFloor
		spawner.MaybePreplaceForFloor((int)floorIndex, col);
	}
}

