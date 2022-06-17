using UnityEngine;
using UnityEngine.Events;

#nullable enable

public class Encounter : MonoBehaviour {
	public float weight;
	public float baseDuration;
	public Sprite? backgroundSprite;
	public Sprite? foregroundSprite;
	// have to do this instead of simple timer because encounter
	// duration is modified by ship systems performance
	[Range(0f, 1f)] public float completion;
	public UnityEvent<Encounter>	OnComplete = new();

	public static bool IsCompatible(Encounter a, Encounter b) => (a.backgroundSprite == null || b.backgroundSprite == null) && (a.foregroundSprite == null || b.foregroundSprite == null);

	private void OnEnable() {
		completion = 0f;
	}

	public float TimeToCompletion(float dt) => Mathf.InverseLerp(0, baseDuration, dt);

	private void Update() {
		completion += TimeToCompletion(Time.deltaTime);
		if (completion >= 1) {
			OnComplete?.Invoke(this);
			gameObject.SetActive(false);
		}
		completion = Mathf.Clamp01(completion);
	}

}
