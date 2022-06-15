using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class Segment : MonoBehaviour {
	public Rigidbody[] extremities = new Rigidbody[2]; 
	private void Start() {
		var rbs = GetComponentInChildren<Rigidbody>();
		rbs.maxDepenetrationVelocity /= 5f;
	}
}
