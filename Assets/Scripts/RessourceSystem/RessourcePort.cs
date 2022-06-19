using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

#nullable enable

public class RessourcePort : MonoBehaviour {

	private Grabbable? connectedCable = null;
	[HideInInspector] public Plug? connectedPlug;
	public FixedJoint? cableJoint;
	[SerializeField] public Ressource? ressType;
	[SerializeField] private float ejectForce = 10f;
	public Transform? plugTransform;
	public Vector3 ejectionDirection = Vector3.right;
	public Cone ejectionRandomDeviation;
	public Plug.Status flowDirection;

	public UnityEvent OnInteract = new();
	public UnityEvent OnEject = new();

	public float cableConnectDistance = 0.1f;
	private float cableStartTime = 0f;
	public float cableConnectTime = 0.5f;
	public float cableConnectSpeed = 20f;

	[SerializeField] private MeshRenderer? portArrow;
	private Material? arrowMat;

	private void Awake() {
		arrowMat = portArrow?.material;
	}

	private void OnDrawGizmos() {
		Debug.DrawLine(transform.position, gameObject.transform.TransformPoint(new Vector3(1, 0, 0)), Color.red);
	}

	private void Update() {
		UpdateCable();
		if (IsCableConnected) arrowMat?.SetColor("_EmissionColor", new Color(0.25f, 0.25f, 0.25f, 1));
		else arrowMat?.SetColor("_EmissionColor", Color.black);
	}

	private void UpdateCable() {
		if (connectedCable == null || cableJoint!.connectedBody != null)
			return;

		var cableRigibody = connectedCable.GetComponent<Rigidbody>();
		var targetCablePlugPoint = plugTransform?.position ?? transform.position;
		var cableDistance = Vector3.Distance(connectedCable.transform.position, targetCablePlugPoint);

		if (cableDistance < cableConnectDistance) {
			cableJoint!.connectedBody = connectedCable.GetComponent<Rigidbody>();
			return;
		}

		var nextPos = Vector3.Lerp(connectedCable.transform.position, targetCablePlugPoint, (Time.time - cableStartTime) / cableConnectTime);
		var nextVel = Vector3.Normalize(nextPos - cableRigibody.position);
		cableRigibody.velocity = nextVel * cableConnectSpeed;
	}

	public bool IsCableConnected => connectedCable != null;

	public bool CanConnectCable => connectedCable == null;

	public void ConnectCable(Grabbable cable) {
		Debug.Log("connecting cable !");
		connectedCable = cable;
		OnInteract?.Invoke();
		connectedCable.transform.rotation = plugTransform?.rotation ?? transform.rotation;
		connectedCable.transform.localScale = plugTransform?.localScale ?? transform.localScale;
		connectedCable.OnGrab(gameObject);
		connectedPlug = connectedCable.GetComponent<Plug>();
		connectedPlug.status = flowDirection;
	}

	public Grabbable? DisconectCable() {
		if (connectedCable == null) return null;
		OnInteract?.Invoke();
		Debug.Log("disconnecting cable !");
		var disconectedCable = connectedCable;
		connectedCable.OnDrop(gameObject);
		cableJoint!.connectedBody = null;
		connectedCable = null;
		connectedPlug!.status = Plug.Status.None;
		connectedPlug = null;

		return disconectedCable;
	}

	public void EjectCable() {
		var cable = DisconectCable();

		OnEject?.Invoke();

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
