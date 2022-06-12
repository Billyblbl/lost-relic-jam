using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSystem : MonoBehaviour {
	[SerializeField] private float maxStress = 100f;
	[SerializeField] private float stressEjectBaseChance = 30f;
	[SerializeField] private float stressEjectTreshold = 50f;
	[SerializeField] private float stressEjectTimeoutSec = 2f;
	[SerializeField] private float baseStressReduction = 5f;
	[SerializeField] private float stressReductionTimeout = 1f;
	[SerializeField] private List<RessourcePort> ports;
	[SerializeField] private Vector3 perfRequierement = new Vector3(1, 1, 1); // X => COOLANT; Y => ENERGY; Z => FUEL


	[SerializeField] private float stress = 100;
	float lastEjectTry = 0;
	float lastReduction = 0;

	void Update() {

		if (ShouldReduceStress()) {
			Debug.Log("Will Reduce stress !");
			InflictStress(baseStressReduction * -CalcPerformanceLevel());
		}

		if (ShouldEjectCable()) {
			Debug.Log("Will Eject !");
			EjectCableFromRandomPort();
		}
	}

	bool ShouldReduceStress() {
		if (lastReduction + stressReductionTimeout > Time.time) {
			return false;
		}
		lastReduction = Time.time;
		return true;
	}

	RessourcePort EjectCableFromRandomPort() {
		var filledPorts = new List<RessourcePort>();
		ports.ForEach(it => {
			if (it.IsCableConnected)
				filledPorts.Add(it);
		});
		if (filledPorts.Count == 0)
			return null;

		var port = filledPorts[Random.Range(0, filledPorts.Count - 1)];
		port.EjectCable();
		return port;
	}

	bool ShouldEjectCable() {

		if (lastEjectTry + stressEjectTimeoutSec > Time.time) return false;
		lastEjectTry = Time.time;

		if (stress < stressEjectTreshold) return false;
		return Random.Range(0, 100) < stressEjectBaseChance;
	}

	Vector3 CalcSystemRessources() {
		var systemRessources = new Vector3(0, 0, 0);
		ports.ForEach(it => {
			switch (it.ressType) {
				case RessourcePort.RessourceType.COOLANT:
					systemRessources.x += it.IsCableConnected ? 1 : 0;
					break;
				case RessourcePort.RessourceType.ENERGY:
					systemRessources.y += it.IsCableConnected ? 1 : 0;
					break;
				case RessourcePort.RessourceType.FUEL:
					systemRessources.z += it.IsCableConnected ? 1 : 0;
					break;
			};
		});
		return systemRessources;
	}

	public float CalcPerformanceLevel() {
		var systemRessources = CalcSystemRessources();
		return ((systemRessources.x / perfRequierement.x) + (systemRessources.y / perfRequierement.y) + (systemRessources.z / perfRequierement.z)) / 3;
	}

	public void InflictStress(float stressDmg) {
		stress = Mathf.Clamp(stress + stressDmg, 0, maxStress);
	}
}
