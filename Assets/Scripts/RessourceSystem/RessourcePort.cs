using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

#nullable enable

public class RessourcePort : MonoBehaviour {
	public enum RessourceType { COOLANT, ENERGY, FUEL }

	private Grabbable? connectedCable = null;
	Plug? connectedPlug;
	public FixedJoint? cableJoint;
	[SerializeField] public RessourceType ressType = RessourceType.ENERGY;
	[SerializeField] private float ejectForce = 10f;

	public Transform? plugTransform;

	public Vector3 ejectionDirection = Vector3.right;
	public Cone ejectionRandomDeviation;

	private void OnDrawGizmos() {
		Debug.DrawLine(transform.position, gameObject.transform.TransformPoint(new Vector3(1, 0, 0)), Color.red);
	}

	public bool IsCableConnected => connectedCable != null;

	public bool CanConnectCable => connectedCable == null;

	public void ConnectCable(Grabbable cable) {
		Debug.Log("connecting cable !");
		connectedCable = cable;
		connectedCable.transform.position = plugTransform?.position ?? transform.position;
		connectedCable.transform.rotation = plugTransform?.rotation ?? transform.rotation;
		connectedCable.transform.localScale = plugTransform?.localScale ?? transform.localScale;
		cableJoint!.connectedBody = cable.GetComponent<Rigidbody>();
		connectedCable.OnGrab(gameObject);
		connectedPlug = connectedCable.GetComponent<Plug>();
	}

	public Grabbable? DisconectCable() {
		if (connectedCable == null) return null;
		Debug.Log("disconnecting cable !");
		var disconectedCable = connectedCable;
		connectedCable.OnDrop(gameObject);
		cableJoint!.connectedBody = null;
		connectedCable = null;
		connectedPlug = null;

		return disconectedCable;
	}

	public void EjectCable() {
		var cable = DisconectCable();

		var deviationAngles = Random.insideUnitCircle * ejectionRandomDeviation.angle;
		var deviation = Quaternion.Euler(deviationAngles);
		var ejection = Quaternion.LookRotation(ejectionDirection, Vector3.up);
		var deviatedEjection = deviation * ejection * Vector3.forward;
		var ejectDir = plugTransform?.TransformVector(deviatedEjection) ?? transform.TransformVector(deviatedEjection);

		cable?.GetComponent<Rigidbody>().AddForce(ejectDir * ejectForce, ForceMode.Impulse);
	}

}

#if UNITY_EDITOR

[CustomEditor(typeof(RessourcePort))]
public class RessourcePortEditor : Editor {

	ArcHandle arc = new();

	private void OnSceneGUI() {
		var t = (target as RessourcePort)!;

		var newDir = Handles.RotationHandle(Quaternion.LookRotation(t.plugTransform!.TransformDirection(t.ejectionDirection), Vector3.up), t.plugTransform.position);
		t.ejectionDirection = t.plugTransform.InverseTransformVector(newDir * Vector3.forward);
		t.ejectionRandomDeviation.DrawHandle(arc, t.plugTransform.position, newDir);
	}

}

#endif
