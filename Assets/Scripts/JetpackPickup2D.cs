using UnityEngine;

public sealed class JetpackPickup2D : MonoBehaviour
{
	[SerializeField] private float durationSeconds = 8f;
	private bool _consumed;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (_consumed) return;
		if (!other.CompareTag("Player")) return;

		if (other.TryGetComponent<PlayerController2D>(out var controller))
		{
			controller.ApplyJetpack(durationSeconds);
			Debug.Log($"Jetpack applied for {durationSeconds}s");
		}
		else
		{
            Debug.LogWarning("Jetpack: PlayerController2D not found on Player");
		}

		_consumed = true;
		if (TryGetComponent<Collider2D>(out var col)) col.enabled = false;
		enabled = false;
		Destroy(gameObject);
	}
}