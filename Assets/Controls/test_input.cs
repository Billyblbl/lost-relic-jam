using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable enable

public class test_input : MonoBehaviour {

	public InputActionReference? input;

	private void Update() {
		if (input?.action.triggered ?? false) {
			Debug.Log("action");
		}
	}

}
