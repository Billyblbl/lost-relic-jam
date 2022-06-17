using UnityEngine;
using System.Linq;

#nullable enable

public class Plug : MonoBehaviour {
	public Rigidbody? rb;
	public FixedJoint? joint;
	public enum Status {
		None,
		Source,
		System
	}
	public Status status;
	public Cable? cable;

	public bool providingRessource { get => status == Status.System && cable!.ends.FirstOrDefault(p => p.status == Status.Source) != null; }

}
