using UnityEngine;

#nullable enable

public class Plug : MonoBehaviour {
	public Rigidbody? rb;
	public FixedJoint? joint;
	public enum PlugStatus {
		None,
		Source,
		System
	}
	public PlugStatus plugStatus;
}
