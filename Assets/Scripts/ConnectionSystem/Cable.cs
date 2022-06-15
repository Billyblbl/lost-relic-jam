using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#nullable enable

public class Cable : MonoBehaviour {
	public int segmentCount = 1;
	public float jointOverlaps = 0.1f;
	public Ressource? ressource;
	public List<Plug> ends = new();
	public bool generate = false;
	public List<Segment?> links = new();

	public List<Segment?> Generate() {
		links = new();
		if (ressource?.cableSegmentPrefab != null) for (int i = 0; i < segmentCount; i++) {

			Segment newLink;
			if (Application.isEditor){
				newLink = ((GameObject)PrefabUtility.InstantiatePrefab(ressource.cableSegmentPrefab.gameObject, transform)).GetComponent<Segment>();
				newLink.transform.localPosition = Vector3.zero;
				newLink.transform.localRotation = Quaternion.identity;
			} else {
				newLink = Instantiate(ressource.cableSegmentPrefab, transform.position, transform.rotation);
				newLink.transform.parent = transform;
			}
			newLink.transform.localScale = ressource.cableSegmentPrefab.transform.localScale;
			newLink.gameObject.SetActive(true);

			if (i > 0) {
				var prev = links[i - 1];
				var joint = newLink.extremities[0].gameObject.AddComponent<FixedJoint>();
				joint.gameObject.name = $"Connected extremity {i}";
				joint.connectedBody = prev?.extremities[1];
				joint.enablePreprocessing = false;
			}
			newLink.gameObject.name = $"Link {i}";
			links.Add(newLink);
		}

		//Generate ends
		if (ressource?.cableEndPrefab != null && links.Count > 0) {
			var extremities = new Rigidbody[2] {
				links[0]?.extremities[0]!,
				links[links.Count-1]?.extremities[(links[links.Count-1]?.extremities.Length ?? 1) - 1]!
			};
			var plugs = new Plug[2] {
				((GameObject)PrefabUtility.InstantiatePrefab(ressource.cableEndPrefab.gameObject, transform)).GetComponent<Plug>(),// Instantiate(ressource.cableEndPrefab, extremities[0]?.transform.position ?? Vector3.zero, extremities[0]?.transform.rotation ?? Quaternion.identity, transform),
				((GameObject)PrefabUtility.InstantiatePrefab(ressource.cableEndPrefab.gameObject, transform)).GetComponent<Plug>()//Instantiate(ressource.cableEndPrefab, extremities[1]?.transform.position ?? Vector3.zero, Quaternion.Euler(180f, 0f, 0f) * (extremities[0]?.transform.rotation ?? Quaternion.identity), transform)
			};

			void InitPlug(Plug plug, Rigidbody extremity, bool upsideDown = false) {
				plug.transform.position = extremity.transform.position;
				plug.transform.rotation = (upsideDown ? Quaternion.Euler(180f, 0f, 0f) : Quaternion.identity) * extremity.transform.rotation;
				plug.joint!.connectedBody = extremity;
				plug.joint!.enablePreprocessing = false;
				plug.cable = this;
			}

			InitPlug(plugs[0], extremities[0]);
			InitPlug(plugs[1], extremities[1], upsideDown:true);

			ends.AddRange(plugs);
		}

		return links;
	}

	public void PlaceLinks() {
		var segmentPrefab = ressource?.cableSegmentPrefab;
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
