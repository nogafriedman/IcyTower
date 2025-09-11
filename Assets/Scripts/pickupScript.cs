using UnityEngine;

public sealed class PowerUpPickup2D : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		Debug.Log($"PU trigger with {other.name}, tag={other.tag}");
		if (!other.CompareTag("Player"))
		{
			return;
		}
		// success message: player touched it
		Debug.Log("Speed Boost collected by Player");

		PlayerController2D controller = other.GetComponent<PlayerController2D>();
		if (controller != null)
		{
			controller.ApplyTemporaryMovementBoost(1.5f, 6f);
			Debug.Log("Applied x1.5 speed for 6s");
		}
		else
		{
			Debug.LogWarning("PlayerController2D not found on the Player.");
		}

		Destroy(gameObject);
	}
}
