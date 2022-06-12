using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class Cable : MonoBehaviour {
	// public float length = 1;
	public int segmentCount = 1;
	// public float segmentLength = 1f;
	public float jointOverlaps = 0.1f;
	public Segment? segmentPrefab;
	public Plug? endsPrefabs;
	public List<Plug> ends = new();
	public bool generate = true;
	public List<Segment?> links = new();

	public List<Segment?> Generate() {
		links = new();
		if (segmentPrefab != null) for (int i = 0; i < segmentCount; i++) {
			var newLink = Instantiate(segmentPrefab, transform.position, transform.rotation);
			newLink.transform.parent = transform;
			newLink.transform.localScale = segmentPrefab.transform.localScale;
			newLink.gameObject.SetActive(true);
			if (i > 0) {
				var prev = links[i - 1];
				var joint = newLink.extremities[0].gameObject.AddComponent<FixedJoint>();
				joint.gameObject.name = $"Connected extremity {i}";
				joint.connectedBody = prev?.extremities[1];
			}
			newLink.gameObject.name = $"Link {i}";
			links.Add(newLink);
		}

		//Generate ends
		if (endsPrefabs != null && links.Count > 0) {
			var extremities = new Rigidbody[2] {
				links[0]?.extremities[0]!,
				links[links.Count-1]?.extremities[(links[links.Count-1]?.extremities.Length ?? 1) - 1]!
			};
			var plugs = new Plug[2] {
				Instantiate(endsPrefabs, extremities[0]?.transform.position ?? Vector3.zero, extremities[0]?.transform.rotation ?? Quaternion.identity, transform),
				Instantiate(endsPrefabs, extremities[1]?.transform.position ?? Vector3.zero, Quaternion.Euler(180f, 0f, 0f) * (extremities[0]?.transform.rotation ?? Quaternion.identity), transform)
			};

			plugs[0].joint!.connectedBody = extremities[0];
			plugs[1].joint!.connectedBody = extremities[1];

			ends.AddRange(plugs);
		}

		return links;
	}

	public void PlaceLinks() {
		var nextDirection = segmentPrefab?.extremities[1].transform.position - segmentPrefab?.extremities[0].transform.position ?? Vector3.down;
		nextDirection -= nextDirection * jointOverlaps;
		for (int i = 0; i < links.Count; i++) {
			if (i > 0) {
				var current = links[i];
				var prev = links[i - 1];
				if (current == null || prev == null) continue;
				current.transform.localPosition = prev.transform.localPosition;
				current.transform.Translate(nextDirection * transform.localScale.y, Space.Self);
			}
		}

		foreach (var end in ends) {
			end.joint!.transform.position = end.joint.connectedBody.transform.position;
		}
	}

	public void Clear(bool immediate = false) {
		foreach(var rb in links) if (rb != null) if (immediate) {
			DestroyImmediate(rb.gameObject);
		} else {
			Destroy(rb.gameObject);
		}

		foreach(var end in ends) if (end.rb != null) if (immediate) {
			DestroyImmediate(end.rb.gameObject);
		} else {
			Destroy(end.rb.gameObject);
		}

		ends.Clear();
		links.Clear();
	}

	IEnumerator EditorGenerate() {
		yield return null;
		if (generate) {
			Clear(true);
			Generate();
		}
 		PlaceLinks();
	}

	private void OnValidate() {
		if (gameObject.activeSelf && gameObject.activeInHierarchy) StartCoroutine(EditorGenerate());
	}
}