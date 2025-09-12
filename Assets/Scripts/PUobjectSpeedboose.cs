using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PUobjectSpeedboose : MonoBehaviour
{
	[SerializeField] private float multiplier = 1.6f;	// how much faster
	[SerializeField] private float duration = 5f;		// how many seconds

	private void Reset()
	{
		var pickupCollider = GetComponent<Collider2D>();
		if (pickupCollider != null)
		{
			pickupCollider.isTrigger = true;
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Player"))
		{
			return;
		}

		var increasePlayerSpeed = other.GetComponent<IncreasePlayerSpeed>();
		if (increasePlayerSpeed != null)
		{
			increasePlayerSpeed.ApplyBoost(multiplier, duration);
		}

		Destroy(gameObject);
	}
}
