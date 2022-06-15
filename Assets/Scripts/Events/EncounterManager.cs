using UnityEngine;
using System.Linq;

#nullable enable

public class EncounterManager : MonoBehaviour {

	public float encounterDelay = 30f;
	public BlendingSprite background;
	public BlendingSprite foreground;
	[NonReorderable] public Encounter[] encounters = new Encounter[0];
	public bool startEncounterOnStart = false;
	(float start, float end)[] bandwiths = new (float, float)[0];
	float totalWeights;
	float lastPing = 0;

	public bool autoFetchEncountersInChildren = false;

	private void OnValidate() {
		if (!autoFetchEncountersInChildren) return;

		encounters = GetComponentsInChildren<Encounter>()
			.Where(e => !encounters.Contains(e))
			.Concat(encounters)
			.Where(e => e != null)
			.ToArray();
	}

	private void Start() {
		var current = 0f;
		bandwiths = encounters.Select(e => (0f, e.weight)).ToArray();
		for (int i = 0; i < encounters.Length; i++) {
			bandwiths[i].start += current;
			bandwiths[i].end += current;
			current += encounters[i].weight;
		}
		totalWeights = current;

		if (startEncounterOnStart) Ping();
	}

	private void OnEnable() {
		lastPing = Time.time;
	}

	public Encounter? ChooseRandomEncounter() {
		var weight = Random.Range(0f, totalWeights);
		return encounters
			.Select((e, i) => (e, i))
			.FirstOrDefault(t => weight >= bandwiths[t.i].start && weight < bandwiths[t.i].end).e;
	}

	public void Trigger(Encounter encounter) {
		encounter.gameObject.SetActive(true);
		if (encounter.backgroundSprite != null) background.SwitchTo(encounter.backgroundSprite);
		if (encounter.foregroundSprite != null) foreground.SwitchTo(encounter.foregroundSprite);
	}

	void Ping() {
		lastPing = Time.time;
		Debug.Log("Trying to trigger an encounter");
		var newEncounter = ChooseRandomEncounter();
		Debug.LogFormat("Chosen encounter {0}", newEncounter?.gameObject.name);
		if (newEncounter != null && encounters.All(encounter => !encounter.isActiveAndEnabled || Encounter.IsCompatible(newEncounter, encounter))) {
			Debug.LogFormat("Triggering encounter {0}", newEncounter.gameObject.name);
			Trigger(newEncounter);
		}
	}

	private void Update() {
		var active = encounters.Where(e => e.isActiveAndEnabled);

		if (background.front?.sprite != null && !active.Any(e => e.backgroundSprite != null))
			background.SwitchTo(null);
		if (foreground.front?.sprite != null && !active.Any(e => e.foregroundSprite != null))
			foreground.SwitchTo(null);

		background.UpdateBlend(Time.deltaTime);
		foreground.UpdateBlend(Time.deltaTime);

		if (Time.time > lastPing + encounterDelay) Ping();
	}
}
