using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourceSystemManager : MonoBehaviour {
	public enum ShipSystemType { SHIELD, ENGINE, WEAPONS };

	[SerializeField] private ShipSystem shield;
	[SerializeField] private ShipSystem engine;
	[SerializeField] private ShipSystem weapons;
	[SerializeField] private float maxHP = 100;

	[SerializeField] private float hp = 0;

	void Start() {
		hp = maxHP;
	}

	private ShipSystem GetSystemByType(ShipSystemType sstype) {
		ShipSystem res;

		switch (sstype) {
			case ShipSystemType.SHIELD:
				res = shield;
				break;
			case ShipSystemType.ENGINE:
				res = engine;
				break;
			default:
				res = weapons;
				break;
		}

		return res;
	}

	public void InflictDamage(float dmg) {
		hp -= Mathf.Clamp(dmg, 0, maxHP);
	}

	public void DoPerformanceTest(ShipSystemType ssType, float expectedPerformance, float hpPenalty, float stressPenalty) {
		var targetedSystem = GetSystemByType(ssType);
		var targetSystemPerf = targetedSystem.CalcPerformanceLevel();

		var hpCoef = Mathf.Clamp(expectedPerformance - targetSystemPerf / expectedPerformance, 0, 1);
		var stressCoef = Mathf.Clamp((expectedPerformance * 2) - targetSystemPerf / (expectedPerformance * 2), 0, 1);

		InflictDamage(hpPenalty * hpCoef);
		targetedSystem.InflictStress(stressPenalty * stressCoef);
	}
}
