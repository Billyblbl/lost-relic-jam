using UnityEngine;

#nullable enable

public class Grabbable : MonoBehaviour {
	[SerializeField] private Collider? managedCollider;
	[SerializeField] public RessourcePort.RessourceType ressType;

	public bool isGrabbed = false;

	public void OnGrab(GameObject grabber) {
		if (managedCollider != null) managedCollider.enabled = false;
		isGrabbed = true;
	}

	public void OnDrop(GameObject grabber) {
		if (managedCollider != null) managedCollider.enabled = true;
		isGrabbed = false;
	}
}
