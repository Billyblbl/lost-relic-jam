using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#nullable enable

public class RandomCableEject : MonoBehaviour {
	public float frequency;
	public Ressource? filter;
	public bool autoFetch = false;
	public RessourcePort[] ports = new RessourcePort[0];

	private void OnValidate() {
		if (!autoFetch) return;
		ports = FindObjectsOfType<RessourcePort>().Where(p => filter == null || p.ressType == filter).ToArray();
	}

	private void Update() {
		if (RandomExtension.RandomProba(frequency * Time.deltaTime)) {
			var choices = ports.Where(p => p.IsCableConnected).ToArray();
			if (choices.Length == 0) return;
			var choice = choices[Random.Range(0, choices.Length)];
			choice.EjectCable();
		}
	}
}
