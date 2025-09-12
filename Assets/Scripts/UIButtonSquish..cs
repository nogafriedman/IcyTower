using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UIButtonSquish : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
	[SerializeField] private Vector3 pressedScale = new Vector3(0.96f, 0.96f, 1f);
	[SerializeField] private float springSpeed = 18f;

	private Vector3 _normalScale;
	private Vector3 _targetScale;

	private void Awake()
	{
		_normalScale = transform.localScale;
		_targetScale = _normalScale;
	}

	private void Update()
	{
		// smooth return to target scale
		transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * springSpeed);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_targetScale = pressedScale;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_targetScale = _normalScale;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_targetScale = _normalScale;
	}
}
