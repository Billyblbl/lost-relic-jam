using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#nullable enable

public class ShipSystem : MonoBehaviour {
	[SerializeField] private float maxStress = 100f;
	[SerializeField] private float stressEjectBaseChance = 30f;
	[SerializeField] private float stressEjectTreshold = 50f;
	[SerializeField] private float stressEjectTimeoutSec = 2f;
	[SerializeField] private float baseStressReduction = 5f;
	[SerializeField] private float stressReductionTimeout = 1f;
	[SerializeField] private List<RessourcePort> ports = new();
	[SerializeField] private Vector3 perfRequierement = new Vector3(1, 1, 1); //X => ENERGY ; Y => FUEL;Z => COOLANT


	[SerializeField] private List<GameObject> indicatorLights = new();
	[SerializeField] private Color minStressColor;
	[SerializeField] private Color maxStressColor;


	[SerializeField] private float stress = 100;
	float lastEjectTry = 0;
	float lastReduction = 0;

	void Update() {

		if (ShouldReduceStress()) {
			Debug.LogFormat("{0}: Will Reduce stress ! reduction: {1}", gameObject.name, baseStressReduction * -CalcPerformanceLevel());
			InflictStress(baseStressReduction * -CalcPerformanceLevel());
		}

		if (ShouldEjectCable()) {
			Debug.Log("Will Eject !");
			EjectCableFromRandomPort();
		}
		UpdateIndicatorLight();
		Debug.LogFormat("{0}: performance: {1}", gameObject.name, CalcPerformanceLevel());

	}

	void UpdateIndicatorLight()
    {
		var stressColor = Color.Lerp(minStressColor, maxStressColor, stress / maxStress);
		indicatorLights.ForEach(it =>
		{
			it.GetComponent<Light>().color = stressColor;
			it.GetComponent<MeshRenderer>()?.material.SetColor("_EmissionColor", stressColor);
		});
	
	}

	bool ShouldReduceStress() {
		if (lastReduction + stressReductionTimeout > Time.time) {
			return false;
		}
		lastReduction = Time.time;
		return true;
	}

	RessourcePort? EjectCableFromRandomPort() {
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

	Vector3Int CalcSystemRessources() => ports
		.Where(p => p.IsCableConnected && (p.connectedPlug?.providingRessource ?? false))
		.Select(p => p.ressType?.unit ?? Vector3Int.zero)
		.Aggregate(Vector3Int.zero, (u1, u2) => u1 + u2);

	// TODO check with alex for this calculation
	public float CalcPerformanceLevel() {
		var systemRessources = CalcSystemRessources();
		Debug.LogFormat("{0}: systemRessources : {1}", gameObject.name, systemRessources);

		var nRequieredRessources = (perfRequierement.x != 0 ? perfRequierement.x : 0) +
			(perfRequierement.y != 0 ? perfRequierement.y : 0) +
			(perfRequierement.z != 0 ? perfRequierement.z : 0);
		if (nRequieredRessources == 0) return 0;

		var energyPerfs = perfRequierement.x != 0 ? systemRessources.x / perfRequierement.x : 0;
		var fuelPerfs = perfRequierement.y != 0 ? systemRessources.y / perfRequierement.y : 0;
		var coolantPerfs = perfRequierement.z != 0 ? systemRessources.z / perfRequierement.z : 0;

		Debug.LogFormat("{0}: perfs : {1} {2} {3} / {4}", gameObject.name, energyPerfs, fuelPerfs, coolantPerfs, nRequieredRessources);

		return (energyPerfs + fuelPerfs + coolantPerfs) / nRequieredRessources;
	}

	public void InflictStress(float stressDmg) {
		stress = Mathf.Clamp(stress + stressDmg, 0, maxStress);
		Debug.LogFormat("{0}: stress new stress {1} ({2})", gameObject.name, stress, stressDmg);
	}
}
