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
	// public Transform? endsPrefabs;

	// [System.Serializable] public struct End {
	// 	public Transform? obj;
	// 	// public Socket? pluggedIn;
	// }

	// public End[] ends = new End[2];

	public bool generate = true;
	public List<Segment> links = new();

	public List<Segment> Generate() {
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
				joint.connectedBody = prev.extremities[1];
			}
			newLink.gameObject.name = $"Link {i}";
			links.Add(newLink);
		}
		return links;
	}

	public void PlaceLinks() {
		var nextDirection = segmentPrefab?.extremities[1].transform.position - segmentPrefab?.extremities[0].transform.position ?? Vector3.down;
		nextDirection -= nextDirection * jointOverlaps;
		for (int i = 0; i < segmentCount; i++) {
			if (i > 0) {
				var current = links[i];
				var prev = links[i - 1];
				current.transform.localPosition = prev.transform.localPosition;
				current.transform.Translate(nextDirection * transform.localScale.y, Space.Self);
			}
		}
	}

	public void Clear(bool immediate = false) {
		foreach(var rb in links) if (immediate) {
			DestroyImmediate(rb.gameObject);
		} else {
			Destroy(rb.gameObject);
		}
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
