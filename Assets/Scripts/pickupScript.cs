using UnityEngine;

public sealed class PowerUpPickup2D : MonoBehaviour
{
	private bool _consumed;

	private void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log($"[PU] trigger with {other.name}, tag={other.tag}");

		if (_consumed) return;
		if (!other.CompareTag("Player"))
		{
			Debug.Log("[PU] non-player ignored");
			return;
		}

		var controller = other.GetComponent<PlayerController2D>();
		if (controller != null)
		{
			Debug.Log("✅ Speed Boost collected by Player");
			controller.ApplyTemporaryMovementBoost(1.5f, 6f);
			Debug.Log("✅ Applied x1.5 speed for 6s");
		}
		else
		{
			Debug.LogWarning("⚠️ PlayerController2D not found on Player");
		}

		_consumed = true;
		var col = GetComponent<Collider2D>();
		if (col) col.enabled = false;
		enabled = false;

		Destroy(gameObject);
	}
}
