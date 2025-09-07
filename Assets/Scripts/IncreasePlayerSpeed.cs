using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class IncreasePlayerSpeed : MonoBehaviour
{
	[SerializeField] private float multiplier = 1f;
	private Coroutine routine;

	public float CurrentMultiplier => multiplier;

	public void ApplyBoost(float boostMultiplier, float duration)
	{
		if (routine != null)
		{
			StopCoroutine(routine);
		}
		routine = StartCoroutine(Boost(boostMultiplier, duration));
	}

	private IEnumerator Boost(float m, float d)
	{
		float old = multiplier;
		multiplier = m;
		yield return new WaitForSeconds(d);
		multiplier = old;
		routine = null;
	}
}
