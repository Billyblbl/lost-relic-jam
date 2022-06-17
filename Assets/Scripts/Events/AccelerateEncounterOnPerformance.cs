using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class AccelerateEncounterOnPerformance : MonoBehaviour {
	public Encounter? encounter;
	public ShipSystem? performanceReference;
	public float speedFactor;

	private void OnEnable() {
		enabled = encounter != null && performanceReference != null;
	}

	private void Update() {
		encounter!.completion += encounter.TimeToCompletion(performanceReference!.CalcPerformanceLevel() * speedFactor * Time.deltaTime);
	}

}
