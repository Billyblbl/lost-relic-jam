using UnityEngine;
using UnityEngine.Events;

#nullable enable

public class RandomDamageGenerator : MonoBehaviour {

	public float baseImpactChance = .5f;
	public Range<float> impactDamageRange;
	public Range<float> impactStressRange;
	public UnityEvent OnImpact = new();
	public RessourceSystemManager? manager;
	public RessourceSystemManager.PerformanceTest baseTest;

	public ShipSystem? performanceReference;
	public float performanceChanceModifier;

	public float impactChance { get => baseImpactChance + (performanceReference != null ? performanceChanceModifier * performanceReference.CalcPerformanceLevel() : 0); }

	private void Update() {
		if (Random.Range(0f, 1f) * Time.deltaTime > 1f - baseImpactChance) {
			RessourceSystemManager.PerformanceTest test = baseTest;

			test.hpPenalty = Random.Range(impactDamageRange.start, impactDamageRange.end);
			test.stressPenalty = Random.Range(impactStressRange.start, impactStressRange.end);

			manager?.DoPerformanceTest(test);
			OnImpact?.Invoke();

		}
	}

}
