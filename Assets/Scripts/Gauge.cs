using UnityEngine;
using UnityEngine.Events;

#nullable enable

[System.Serializable] public struct Gauge {
	public float max;
	[SerializeField] private float _current;
	public float current {get => _current; set {
		if (value < _current) lastLoss = Time.time;
		_current = Mathf.Clamp(value, -1, max);
		OnValueChange?.Invoke(current);
		OnFractionChange?.Invoke(current / max);
		if (current <= 0f) {
			OnDepleted?.Invoke();
			OnDepleted?.RemoveAllListeners();
		}
	}}

	public float regen;
	public float regenDelay;
	float lastLoss;
	public UnityEvent<float>	OnValueChange;
	public UnityEvent<float>	OnFractionChange;
	public UnityEvent	OnDepleted;

	public void Update(float t, float dt) {
		if (current < max && lastLoss + regenDelay < t)
			current = Mathf.Clamp(current + regen * dt, 0, max);
	}

}
