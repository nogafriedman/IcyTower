using UnityEngine;

public class PlatformIndex : MonoBehaviour
{
	public int floorIndex = 0;
	private void Start()
	{
		var col = GetComponent<Collider2D>();
		if (!col) return;

#if UNITY_2023_1_OR_NEWER
		var spawners = UnityEngine.Object.FindObjectsByType<PowerUpSpawner2D>(FindObjectsSortMode.None);
#else
	var spawners = UnityEngine.Object.FindObjectsOfType<PowerUpSpawner2D>();
#endif

		for (int i = 0; i < spawners.Length; i++)
		{
			spawners[i].MaybePreplaceForFloor((int)floorIndex, col);
		}
	}
}

