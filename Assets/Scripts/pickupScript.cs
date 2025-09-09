using UnityEngine;

public sealed class PowerUpPickup2D : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Player"))
		{
			return;
		}

		Debug.Log("Power-Up collected by Player");
		Destroy(gameObject);
	}
}
