using UnityEngine;

public class Grabbable : MonoBehaviour {
	[SerializeField] private Collider managedCollider;
	[SerializeField] public RessourcePort.RessourceType ressType;

	public bool isGrabbed = false;

	public void OnGrab(GameObject grabber) {
		managedCollider.enabled = false;
		isGrabbed = true;
	}

	public void OnDrop(GameObject grabber) {
		managedCollider.enabled = true;
		isGrabbed = false;
	}
}
