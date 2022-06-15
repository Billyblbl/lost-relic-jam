using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class RessourceSystemManager : MonoBehaviour {

	[System.Serializable] public struct PerformanceTest {
		public ShipSystem? target;
		public float expectedPerformance;
		public float hpPenalty;
		public float stressPenalty;
	}

	public ShipSystem[] systems = new ShipSystem[0];

	public Gauge HP;

	private void Update() {
		HP.Update(Time.time, Time.deltaTime);
	}

	public void InflictDamage(float dmg) {
		HP.current -= dmg;
	}

	public (float, float) DoPerformanceTest(PerformanceTest test) => DoPerformanceTest(
		test.target!,
		test.expectedPerformance,
		test.hpPenalty,
		test.stressPenalty
	);

	public (float, float) DoPerformanceTest(ShipSystem system, float expectedPerformance, float hpPenalty, float stressPenalty) {

		var targetSystemPerf = system.CalcPerformanceLevel();

		var hpCoef = Mathf.Clamp(expectedPerformance - targetSystemPerf / expectedPerformance, 0, 1);
		var stressCoef = Mathf.Clamp((expectedPerformance * 2) - targetSystemPerf / (expectedPerformance * 2), 0, 1);

		InflictDamage(hpPenalty * hpCoef);
		system.InflictStress(stressPenalty * stressCoef);

		return (hpPenalty * hpCoef, stressPenalty * stressCoef);
	}
}
